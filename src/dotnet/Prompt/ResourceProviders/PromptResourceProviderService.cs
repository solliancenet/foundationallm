using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Prompt.Models.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Prompt.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Prompt resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class PromptResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Prompt)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<PromptResourceProviderService> logger)
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
            PromptResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, PromptReference> _promptReferences = [];

        private const string PROMPT_REFERENCES_FILE_NAME = "_prompt-references.json";
        private const string PROMPT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Prompt}/{PROMPT_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Prompt;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default);
                var promptReferenceStore = JsonSerializer.Deserialize<PromptReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _promptReferences = new ConcurrentDictionary<string, PromptReference>(
                    promptReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new PromptReferenceStore { PromptReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await LoadPrompts(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<PromptBase>>> LoadPrompts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                var prompts = (await Task.WhenAll(
                        _promptReferences.Values
                            .Where(pr => !pr.Deleted)
                            .Select(pr => LoadPrompt(pr))))
                  .Where(pr => pr != null)
                  .ToList();

                return prompts.Select(prompt => new ResourceProviderGetResult<PromptBase>() { Resource = prompt, Actions = [], Roles = [] }).ToList();
            }
            else
            {
                PromptBase? prompt;
                if (!_promptReferences.TryGetValue(instance.ResourceId, out var promptReference))
                {
                    prompt = await LoadPrompt(null, instance.ResourceId);
                    if (prompt != null)
                    {
                        return [new ResourceProviderGetResult<PromptBase>() { Resource = prompt, Actions = [], Roles = [] }];
                    }
                    return [];
                }

                if (promptReference.Deleted)
                {
                    throw new ResourceProviderException(
                        $"Could not locate the {instance.ResourceId} prompt resource.",
                        StatusCodes.Status404NotFound);
                }

                prompt = await LoadPrompt(promptReference);
                if (prompt != null)
                {
                    return [new ResourceProviderGetResult<PromptBase>() { Resource = prompt, Actions = [], Roles = [] }];
                }
                return [];
            }
        }

        private async Task<PromptBase?> LoadPrompt(PromptReference? promptReference, string? resourceId = null)
        {
            if (promptReference != null || !string.IsNullOrEmpty(resourceId))
            {
                promptReference ??= new PromptReference
                {
                    Name = resourceId!,
                    Type = PromptTypes.Multipart,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };
                if (await _storageService.FileExistsAsync(_storageContainerName, promptReference.Filename, default))
                {
                    var fileContent =
                        await _storageService.ReadFileAsync(_storageContainerName, promptReference.Filename, default);
                    var prompt = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               promptReference.PromptType,
                               _serializerSettings) as PromptBase
                           ?? throw new ResourceProviderException($"Failed to load the prompt {promptReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        promptReference.Type = prompt.Type!;
                        _promptReferences.AddOrUpdate(promptReference.Name, promptReference, (k, v) => promptReference);
                    }

                    return prompt;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _promptReferences.TryRemove(promptReference.Name, out _);
                    return null;
                }
            }
            throw new ResourceProviderException($"Could not locate the {promptReference.Name} prompt resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await UpdatePrompt(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest),
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdatePrompt(ResourcePath resourcePath, string serializedPrompt, UnifiedUserIdentity userIdentity)
        {
            var prompt = JsonSerializer.Deserialize<PromptBase>(serializedPrompt)
                ?? throw new ResourceProviderException("The object definition is invalid.");

            if (_promptReferences.TryGetValue(prompt.Name!, out var existingPromptReference)
                && existingPromptReference!.Deleted)
                throw new ResourceProviderException($"The prompt resource {existingPromptReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != prompt.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var promptReference = new PromptReference
            {
                Name = prompt.Name!,
                Type = prompt.Type!,
                Filename = $"/{_name}/{prompt.Name}.json",
                Deleted = false
            };

            prompt.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            if (existingPromptReference == null)
                prompt.CreatedBy = userIdentity.UPN;
            else
                prompt.UpdatedBy = userIdentity.UPN;

            await _storageService.WriteFileAsync(
                _storageContainerName,
                promptReference.Filename,
                JsonSerializer.Serialize<PromptBase>(prompt, _serializerSettings),
                default,
                default);

            _promptReferences.AddOrUpdate(promptReference.Name, promptReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (prompt as PromptBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                PromptResourceTypeNames.Prompts => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    PromptResourceProviderActions.CheckName => CheckPromptName(serializedAction),
                    PromptResourceProviderActions.Purge => await PurgeResource(resourcePath),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckPromptName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _promptReferences.Values.Any(ar => ar.Name == resourceName!.Name)
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
            var resourceName = resourcePath.ResourceTypeInstances.Last().ResourceId!;
            if (_promptReferences.TryGetValue(resourceName, out var agentReference))
            {
                if (agentReference.Deleted)
                {
                    // Delete the resource file from storage.
                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        agentReference.Filename,
                        default);

                    // Remove this resource reference from the store.
                    _promptReferences.TryRemove(resourceName, out _);

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        PROMPT_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                        default,
                        default);

                    return new ResourceProviderActionResult(true);
                }
                else
                {
                    throw new ResourceProviderException(
                        $"The {resourceName} prompt resource is not soft-deleted and cannot be purged.",
                        StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {resourceName} prompt resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

            /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case PromptResourceTypeNames.Prompts:
                    await DeletePrompt(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeletePrompt(List<ResourceTypeInstance> instances)
        {
            if (_promptReferences.TryGetValue(instances.Last().ResourceId!, out var promptReference)
                || promptReference!.Deleted)
            {
                promptReference.Deleted = true;

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                            StatusCodes.Status404NotFound);
        }

        #endregion

        #endregion
    }
}
