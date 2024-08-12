using Azure.AI.OpenAI.Assistants;
using Azure.ResourceManager.Models;
using FoundationaLLM.Authorization.Models;
using FoundationaLLM.AzureOpenAI.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Gateway.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.AzureOpenAI.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureOpenAIResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AzureOpenAI)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<AzureOpenAIResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AzureOpenAIResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, AzureOpenAIResourceReference> _resourceReferences = [];

        private const string RESOURCE_REFERENCES_FILE_NAME = "_resource-references.json";
        private const string RESOURCE_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{RESOURCE_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AzureOpenAI;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, RESOURCE_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, RESOURCE_REFERENCES_FILE_PATH, default);
                var resourceReferenceStore = JsonSerializer.Deserialize<ResourceReferenceStore<AzureOpenAIResourceReference>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _resourceReferences = new ConcurrentDictionary<string, AzureOpenAIResourceReference>(
                    resourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    RESOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<AzureOpenAIResourceReference> { ResourceReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => await LoadAssistantUserContexts(resourcePath.ResourceTypeInstances[0]),
                AzureOpenAIResourceTypeNames.FileUserContexts => await LoadFileUserContexts(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<AssistantUserContext>>> LoadAssistantUserContexts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                var userContexts = (await Task.WhenAll(
                        _resourceReferences.Values
                            .Where(r => !r.Deleted)
                            .Select(r => LoadAssistantUserContext(r))))
                  .Where(r => r != null)
                  .ToList();

                return userContexts.Select(r => new ResourceProviderGetResult<AssistantUserContext>() { Resource = r!, Actions = [], Roles = [] }).ToList();
            }
            else
            {
                AssistantUserContext? userContext;
                if (!_resourceReferences.TryGetValue(instance.ResourceId, out var userContextReference))
                {
                    userContext = await LoadAssistantUserContext(null, instance.ResourceId);
                    if (userContext != null)
                    {
                        return [new ResourceProviderGetResult<AssistantUserContext>() { Resource = userContext, Actions = [], Roles = [] }];
                    }
                    return [];
                }

                if (userContextReference.Deleted)
                {
                    throw new ResourceProviderException(
                        $"Could not locate the {instance.ResourceId} resource.",
                        StatusCodes.Status404NotFound);
                }

                userContext = await LoadAssistantUserContext(userContextReference);
                if (userContext != null)
                {
                    return [new ResourceProviderGetResult<AssistantUserContext>() { Resource = userContext, Actions = [], Roles = [] }];
                }
                return [];
            }
        }

        private async Task<AssistantUserContext?> LoadAssistantUserContext(AzureOpenAIResourceReference? resourceReference, string? resourceId = null)
        {
            if (resourceReference != null || !string.IsNullOrEmpty(resourceId))
            {
                resourceReference ??= new AzureOpenAIResourceReference
                {
                    Name = resourceId!,
                    Type = AzureOpenAITypes.AssistantUserContext,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };
                if (await _storageService.FileExistsAsync(_storageContainerName, resourceReference.Filename, default))
                {
                    var fileContent =
                        await _storageService.ReadFileAsync(_storageContainerName, resourceReference.Filename, default);
                    var userContext = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               resourceReference.ResourceType,
                               _serializerSettings) as AssistantUserContext
                           ?? throw new ResourceProviderException($"Failed to load the resource {resourceReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        resourceReference.Type = userContext.Type!;
                        _resourceReferences.AddOrUpdate(resourceReference.Name, resourceReference, (k, v) => resourceReference);
                    }

                    return userContext;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _resourceReferences.TryRemove(resourceReference.Name, out _);
                    return null;
                }
            }

            throw new ResourceProviderException($"The {_name} resource provider could not locate a resource because of invalid resource identification parameters.",
                StatusCodes.Status400BadRequest);
        }

        private async Task<List<ResourceProviderGetResult<FileUserContext>>> LoadFileUserContexts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                var fileUserContexts = (await Task.WhenAll(
                        _resourceReferences.Values
                            .Where(r => !r.Deleted)
                            .Select(r => LoadFileUserContext(r))))
                  .Where(r => r != null)
                  .ToList();

                return fileUserContexts.Select(r => new ResourceProviderGetResult<FileUserContext>() { Resource = r!, Actions = [], Roles = [] }).ToList();
            }
            else
            {
                FileUserContext? fileUserContext;
                if (!_resourceReferences.TryGetValue(instance.ResourceId, out var fileUserContextReference))
                {
                    fileUserContext = await LoadFileUserContext(null, instance.ResourceId);
                    if (fileUserContext != null)
                    {
                        return [new ResourceProviderGetResult<FileUserContext>() { Resource = fileUserContext, Actions = [], Roles = [] }];
                    }
                    return [];
                }

                if (fileUserContextReference.Deleted)
                {
                    throw new ResourceProviderException(
                        $"Could not locate the {instance.ResourceId} resource.",
                        StatusCodes.Status404NotFound);
                }

                fileUserContext = await LoadFileUserContext(fileUserContextReference);
                if (fileUserContext != null)
                {
                    return [new ResourceProviderGetResult<FileUserContext>() { Resource = fileUserContext, Actions = [], Roles = [] }];
                }
                return [];
            }
        }

        private async Task<FileUserContext?> LoadFileUserContext(AzureOpenAIResourceReference? resourceReference, string? resourceId = null)
        {
            if (resourceReference != null || !string.IsNullOrEmpty(resourceId))
            {
                 resourceReference ??= new AzureOpenAIResourceReference
                {
                    Name = resourceId!,
                    Type = AzureOpenAITypes.FileUserContext,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };
                if (await _storageService.FileExistsAsync(_storageContainerName, resourceReference.Filename, default))
                {
                    var fileContent =
                        await _storageService.ReadFileAsync(_storageContainerName, resourceReference.Filename, default);
                    var userContext = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               resourceReference.ResourceType,
                               _serializerSettings) as FileUserContext
                           ?? throw new ResourceProviderException($"Failed to load the resource {resourceReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        resourceReference.Type = userContext.Type!;
                        _resourceReferences.AddOrUpdate(resourceReference.Name, resourceReference, (k, v) => resourceReference);
                    }

                    return userContext;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _resourceReferences.TryRemove(resourceReference.Name, out _);
                    return null;
                }
            }

            throw new ResourceProviderException($"The {_name} resource provider could not locate a resource because of invalid resource identification parameters.",
                StatusCodes.Status400BadRequest);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => await UpdateAssistantUserContext(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest),
            };

        #region Helpers for UpsertResourceAsync

        private async Task<AssistantUserContextUpsertResult> UpdateAssistantUserContext(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity)
        {
            var assistantUserContext = JsonSerializer.Deserialize<AssistantUserContext>(serializedResource)
                ?? throw new ResourceProviderException("The object definition is invalid.");

            bool resourceExists = false;
            AssistantUserContext updatedAssistantUserContext;
            if (_resourceReferences.TryGetValue(assistantUserContext.Name!, out AzureOpenAIResourceReference? resourceReference))
            {
                // Check if the resource was logically deleted.
                if (resourceReference!.Deleted)
                    throw new ResourceProviderException($"The resource {resourceReference.Name} cannot be added or updated.",
                            StatusCodes.Status400BadRequest);
                resourceExists = true;
            }
            else
                resourceReference = new AzureOpenAIResourceReference
                {
                    Name = assistantUserContext.Name!,
                    Type = assistantUserContext.Type!,
                    Filename = $"/{_name}/{assistantUserContext.Name}.json",
                    Deleted = false
                };

            if (resourcePath.ResourceTypeInstances[0].ResourceId != assistantUserContext.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            assistantUserContext.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
            assistantUserContext.Version = Version.Parse(_instanceSettings.Version);

            var gatewayClient = new GatewayServiceClient(
                await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                    .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var newOpenAIAssistantId = default(string);
            var newOpenAIAssistantThreadId = default(string);

            var incompleteConversations = assistantUserContext.Conversations.Values
                    .Where(c => string.IsNullOrWhiteSpace(c.OpenAIThreadId))
                    .ToList();

            if (incompleteConversations.Count != 1)
                throw new ResourceProviderException($"The Assistant user context {assistantUserContext.Name} contains an incorrect number of incomplete conversations (must be 1). This indicates an inconsistent approach in the resource management flow.");

            if (!resourceExists)
            {
                // Creating a new resource.
                assistantUserContext.CreatedBy = userIdentity.UPN;

                var result = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    resourceReference.Name,
                    new()
                    {
                        { OpenAIAgentCapabilityParameterNames.CreateAssistant, true },
                        { OpenAIAgentCapabilityParameterNames.CreateAssistantThread, true },
                        { OpenAIAgentCapabilityParameterNames.Endpoint, assistantUserContext.Endpoint },
                        { OpenAIAgentCapabilityParameterNames.ModelDeploymentName , assistantUserContext.ModelDeploymentName },
                        { OpenAIAgentCapabilityParameterNames.AssistantPrompt, assistantUserContext.Prompt }
                    });

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantId, out var newOpenAIAssistantIdObject);
                newOpenAIAssistantId = ((JsonElement)newOpenAIAssistantIdObject!).Deserialize<string>();

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantThreadId, out var newOpenAIAssistantThreadIdObject);
                newOpenAIAssistantThreadId = ((JsonElement)newOpenAIAssistantThreadIdObject!).Deserialize<string>();

                assistantUserContext.OpenAIAssistantId = newOpenAIAssistantId;
                assistantUserContext.OpenAIAssistantCreatedOn = DateTimeOffset.UtcNow;

                var conversation = assistantUserContext.Conversations.Values
                    .SingleOrDefault(c => string.IsNullOrWhiteSpace(c.OpenAIThreadId))
                    ?? throw new ResourceProviderException("Could not find a conversation with an empty assistant thread id.");

                conversation.OpenAIThreadId = newOpenAIAssistantThreadId;
                conversation.OpenAIThreadCreatedOn = assistantUserContext.OpenAIAssistantCreatedOn;

                updatedAssistantUserContext = assistantUserContext;
            }
            else
            {
                var existingAssistantUserContext = await LoadAssistantUserContext(resourceReference)
                    ?? throw new ResourceProviderException(
                        $"Could not load the {resourceReference.Name} assistant user context.");

                if (existingAssistantUserContext.Conversations.ContainsKey(incompleteConversations[0].FoundationaLLMSessionId))
                    throw new ResourceProviderException(
                        $"An OpenAI thread was already created for the FoundationaLLM session {incompleteConversations[0].FoundationaLLMSessionId}.",
                        StatusCodes.Status400BadRequest);

                existingAssistantUserContext.Conversations.Add(
                    incompleteConversations[0].FoundationaLLMSessionId,
                    incompleteConversations[0]);

                var result = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    resourceReference.Name,
                    new()
                    {
                        { OpenAIAgentCapabilityParameterNames.AssistantId, assistantUserContext.OpenAIAssistantId! },
                        { OpenAIAgentCapabilityParameterNames.CreateAssistantThread, true },
                        { OpenAIAgentCapabilityParameterNames.Endpoint, assistantUserContext.Endpoint }
                    });

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantThreadId, out var newOpenAIAssistantThreadIdObject);
                newOpenAIAssistantThreadId = ((JsonElement)newOpenAIAssistantThreadIdObject!).Deserialize<string>();

                incompleteConversations[0].OpenAIThreadId = newOpenAIAssistantThreadId;
                incompleteConversations[0].OpenAIThreadCreatedOn = DateTimeOffset.UtcNow;
                assistantUserContext.UpdatedBy = userIdentity.UPN;
                updatedAssistantUserContext = assistantUserContext;
            }

            UpdateBaseProperties(updatedAssistantUserContext, userIdentity);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                resourceReference.Filename,
                JsonSerializer.Serialize<AssistantUserContext>(updatedAssistantUserContext, _serializerSettings),
                default,
                default);

            _resourceReferences.AddOrUpdate(resourceReference.Name, resourceReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                RESOURCE_REFERENCES_FILE_PATH,
                JsonSerializer.Serialize(ResourceReferenceStore<AzureOpenAIResourceReference>.FromDictionary(_resourceReferences.ToDictionary())),
                default,
                default);

            return new AssistantUserContextUpsertResult
            {
                ObjectId = (updatedAssistantUserContext as AssistantUserContext)!.ObjectId,
                NewOpenAIAssistantId = newOpenAIAssistantId,
                NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    ResourceProviderActions.CheckName => CheckResourceName(serializedAction),
                    ResourceProviderActions.Purge => await PurgeResource(resourcePath),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckResourceName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _resourceReferences.Values.Any(r => r.Name == resourceName!.Name)
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed
                };
        }

        private async Task<ResourceProviderActionResult> PurgeResource(ResourcePath resourcePath)
        {
            throw new NotImplementedException("The Azure OpenAI resource cleanup is not implemented.");

#pragma warning disable CS0162 // Unreachable code detected
            var resourceName = resourcePath.ResourceTypeInstances.Last().ResourceId!;
#pragma warning restore CS0162 // Unreachable code detected
            if (_resourceReferences.TryGetValue(resourceName, out var resourceReference))
            {
                if (resourceReference.Deleted)
                {
                    // Delete the resource file from storage.
                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        resourceReference.Filename,
                        default);

                    // Remove this resource reference from the store.
                    _resourceReferences.TryRemove(resourceName, out _);

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        RESOURCE_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(ResourceReferenceStore<AzureOpenAIResourceReference>.FromDictionary(_resourceReferences.ToDictionary())),
                        default,
                        default);

                    return new ResourceProviderActionResult(true);
                }
                else
                {
                    throw new ResourceProviderException(
                        $"The {resourceName} resource is not soft-deleted and cannot be purged.",
                        StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {resourceName} resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case AzureOpenAIResourceTypeNames.AssistantUserContexts:
                    await DeleteAssistantUserContext(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAssistantUserContext(List<ResourceTypeInstance> instances)
        {
            if (_resourceReferences.TryGetValue(instances.Last().ResourceId!, out var resourceReference)
                && !resourceReference.Deleted)
            {
                resourceReference.Deleted = true;

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    RESOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(ResourceReferenceStore<AzureOpenAIResourceReference>.FromDictionary(_resourceReferences.ToDictionary())),
                    default,
                    default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceInternal<T>(ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderOptions? options = null) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                AzureOpenAIResourceTypeNames.FilesContent => ((await LoadFileContent(
                    resourcePath.ResourceTypeInstances[0].ResourceId!,
                    resourcePath.ResourceTypeInstances[1].ResourceId!)) as T)!,
                AzureOpenAIResourceTypeNames.FileUserContexts => ((await LoadFileUserContext(resourcePath.ResourceTypeInstances[0].ResourceId!)) as T)!,
                _ => throw new ResourceProviderException(
                    $"The {resourcePath.MainResourceType} resource type is not supported by the {_name} resource provider.")
            };

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(ResourcePath resourcePath, T resource, UnifiedUserIdentity userIdentity) =>
            resource switch
            {
                FileUserContext fileUserContext => ((await UpdateFileUserContext(fileUserContext, userIdentity)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        private async Task<FileUserContext> LoadFileUserContext(string fileUserContextName)
        {
            _resourceReferences.TryGetValue(fileUserContextName, out AzureOpenAIResourceReference? resourceReference);
            if (resourceReference == null || resourceReference.Deleted)
            {
                // Force a refresh of the references one time to make sure we don't have a stale copy.
                await InitializeInternal();
                _resourceReferences.TryGetValue(fileUserContextName, out resourceReference);

                if (resourceReference == null)
                    throw new ResourceProviderException(
                        $"The resource {fileUserContextName} was not found.",
                        StatusCodes.Status404NotFound);
            }
            return await LoadFileUserContext(resourceReference)
                   ?? throw new ResourceProviderException(
                       $"Could not load the resource {fileUserContextName} resource.",
                       StatusCodes.Status500InternalServerError);
        }

        private async Task<FileContent> LoadFileContent(string fileUserContextName, string openAIFileId)
        {
            var fileUserContext = await LoadFileUserContext(fileUserContextName);
            var fileMapping = fileUserContext.Files.Values
                .SingleOrDefault(f => f.OpenAIFileId == openAIFileId)
                    ?? throw new ResourceProviderException(
                        $"Could not find the file {openAIFileId} in the {fileUserContextName} file user context.",
                        StatusCodes.Status404NotFound);

            var assistantsClient = new AssistantsClient(new Uri(fileUserContext!.Endpoint), DefaultAuthentication.AzureCredential);

            var result = await assistantsClient.GetFileContentAsync(openAIFileId);

            return new FileContent
            {
                Name = openAIFileId,
                OriginalFileName = fileMapping.OriginalFileName,
                ContentType = fileMapping.ContentType,
                BinaryContent = result.Value.ToMemory()
            };
        }

        private async Task<FileUserContextUpsertResult> UpdateFileUserContext(FileUserContext fileUserContext, UnifiedUserIdentity userIdentity)
        {
            bool resourceExists = false;
            FileUserContext updatedFileUserContext;
            if (_resourceReferences.TryGetValue(fileUserContext.Name!, out AzureOpenAIResourceReference? resourceReference))
            {
                // Check if the resource was logically deleted.
                if (resourceReference!.Deleted)
                    throw new ResourceProviderException($"The resource {resourceReference.Name} cannot be added or updated.",
                            StatusCodes.Status400BadRequest);
                resourceExists = true;
            }
            else
                resourceReference = new AzureOpenAIResourceReference
                {
                    Name = fileUserContext.Name!,
                    Type = fileUserContext.Type!,
                    Filename = $"/{_name}/{fileUserContext.Name}.json",
                    Deleted = false
                };

            fileUserContext.ObjectId = $"/instances/{_instanceSettings.Id}/providers/{_name}/{AzureOpenAIResourceTypeNames.FileUserContexts}/{fileUserContext.Name}";

            var gatewayClient = new GatewayServiceClient(
                await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                    .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var newOpenAIFileId = default(string);

            var incompleteFiles = fileUserContext.Files.Values
                    .Where(c => string.IsNullOrWhiteSpace(c.OpenAIFileId))
                    .ToList();

            if (incompleteFiles.Count != 1)
                throw new ResourceProviderException($"The File user context {fileUserContext.Name} contains an incorrect number of incomplete files (must be 1). This indicates an inconsistent approach in the resource management flow.");

            if (!resourceExists)
            {
                fileUserContext.CreatedBy = userIdentity.UPN;

                if (incompleteFiles.Count == 1)
                {
                    var result = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    fileUserContext.AssistantUserContextName,
                    new()
                    {
                        { OpenAIAgentCapabilityParameterNames.CreateAssistantFile, true },
                        { OpenAIAgentCapabilityParameterNames.Endpoint, fileUserContext.Endpoint },
                        { OpenAIAgentCapabilityParameterNames.AttachmentObjectId,  incompleteFiles[0].FoundationaLLMAttachmentObjectId }
                    });

                    result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantFileId, out var newOpenAIFileIdObject);
                    newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();

                    incompleteFiles[0].OpenAIFileId = newOpenAIFileId;
                    incompleteFiles[0].OpenAIFileUploadedOn = DateTimeOffset.UtcNow;
                }

                updatedFileUserContext = fileUserContext;
            }
            else
            {
                var existingFileUserContext = await LoadFileUserContext(resourceReference)
                    ?? throw new ResourceProviderException(
                        $"Could not load the {fileUserContext.Name} file user context.");

                if (existingFileUserContext.Files.ContainsKey(incompleteFiles[0].FoundationaLLMAttachmentObjectId))
                    throw new ResourceProviderException(
                        $"An OpenAI file was already created for the FoundationaLLM attachment {incompleteFiles[0].FoundationaLLMAttachmentObjectId}.",
                        StatusCodes.Status400BadRequest);

                existingFileUserContext.Files.Add(
                    incompleteFiles[0].FoundationaLLMAttachmentObjectId,
                    incompleteFiles[0]);

                var result = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    fileUserContext.AssistantUserContextName,
                    new()
                    {
                        { OpenAIAgentCapabilityParameterNames.CreateAssistantFile, true },
                        { OpenAIAgentCapabilityParameterNames.Endpoint, existingFileUserContext.Endpoint },
                        { OpenAIAgentCapabilityParameterNames.AttachmentObjectId,  incompleteFiles[0].FoundationaLLMAttachmentObjectId }
                    });

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantFileId, out var newOpenAIFileIdObject);
                newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();

                incompleteFiles[0].OpenAIFileId = newOpenAIFileId;
                incompleteFiles[0].OpenAIFileUploadedOn = DateTimeOffset.UtcNow;
                existingFileUserContext.UpdatedBy = userIdentity.UPN;
                updatedFileUserContext = existingFileUserContext;
            }

            UpdateBaseProperties(updatedFileUserContext, userIdentity);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                resourceReference.Filename,
                JsonSerializer.Serialize<FileUserContext>(updatedFileUserContext, _serializerSettings),
                default,
                default);

            _resourceReferences.AddOrUpdate(resourceReference.Name, resourceReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    RESOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(ResourceReferenceStore<AzureOpenAIResourceReference>.FromDictionary(_resourceReferences.ToDictionary())),
                    default,
                    default);

            return new FileUserContextUpsertResult
            {
                ObjectId = (updatedFileUserContext as FileUserContext)!.ObjectId,
                NewOpenAIFileId = newOpenAIFileId!
            };
        }

        #endregion
    }
}
