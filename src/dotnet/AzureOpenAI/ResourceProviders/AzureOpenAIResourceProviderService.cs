using Azure.AI.OpenAI;
using FoundationaLLM.AzureOpenAI.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        : ResourceProviderServiceBase<AzureOpenAIResourceReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            eventNamespacesToSubscribe: null,
            useInternalReferencesStore: true)
    {
        private readonly SemaphoreSlim _localLock = new(1, 1);

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AzureOpenAIResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AzureOpenAI;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderLoadOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => await LoadResources<AssistantUserContext>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult),
                AzureOpenAIResourceTypeNames.FileUserContexts => await LoadResources<FileUserContext>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<AssistantUserContext>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),
                    ResourceProviderActions.Purge => await PurgeResource<AssistantUserContext>(resourcePath),
                    _ => throw new ResourceProviderException(
                        $"The action {resourcePath.Action} is not supported for the resource type {AzureOpenAIResourceTypeNames.AssistantUserContexts} by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                AzureOpenAIResourceTypeNames.AssistantUserContexts => (await LoadResource<T>(
                    resourcePath.MainResourceId!))!,
                AzureOpenAIResourceTypeNames.FilesContent => ((await LoadFileContent(
                    resourcePath.MainResourceId!,
                    resourcePath.ResourceId!)) as T)!,
                AzureOpenAIResourceTypeNames.FileUserContexts => ((await LoadFileUserContext(resourcePath.MainResourceId!)) as T)!,
                _ => throw new ResourceProviderException(
                    $"The {resourcePath.MainResourceTypeName} resource type is not supported by the {_name} resource provider.")
            };

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(ResourcePath resourcePath, T resource, UnifiedUserIdentity userIdentity) =>
            resource switch
            {
                AssistantUserContext assistantUserContext => ((await UpdateAssistantUserContext(assistantUserContext, userIdentity)) as TResult)!,
                FileUserContext fileUserContext => ((await UpdateFileUserContext(fileUserContext, userIdentity)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        private async Task<FileUserContext> LoadFileUserContext(string fileUserContextName)
        {
            try
            {
                await _localLock.WaitAsync();

                var resourceReference = await _resourceReferenceStore!.GetResourceReference(fileUserContextName)
                    ?? throw new ResourceProviderException(
                        $"The resource {fileUserContextName} was not found.",
                        StatusCodes.Status404NotFound);

                return await LoadResource<FileUserContext>(resourceReference)
                    ?? throw new ResourceProviderException(
                        $"The resource {fileUserContextName} has a valid resource reference but cannot be loaded from the storage. This might indicate a missing resource file.",
                        StatusCodes.Status500InternalServerError);

            }
            finally
            {
                _localLock.Release();
            }
        }

        private async Task<FileContent> LoadFileContent(string fileUserContextName, string openAIFileId)
        {
            var fileUserContext = await LoadFileUserContext(fileUserContextName);
            var fileMapping = fileUserContext.Files.Values
                .SingleOrDefault(f => f.Generated && f.OpenAIFileId == openAIFileId)
                    ?? throw new ResourceProviderException(
                        $"Could not find the file {openAIFileId} in the {fileUserContextName} file user context.",
                        StatusCodes.Status404NotFound);

            var azureOpenAIClient = new AzureOpenAIClient(new Uri(fileUserContext!.Endpoint), DefaultAuthentication.AzureCredential);
            var fileClient = azureOpenAIClient.GetFileClient();

            var result = await fileClient.DownloadFileAsync(openAIFileId);

            return new FileContent
            {
                Name = openAIFileId,
                OriginalFileName = fileMapping.OriginalFileName,
                ContentType = fileMapping.ContentType,
                BinaryContent = result.Value.ToMemory()
            };
        }

        #endregion

        #region Resource management

        private async Task<AssistantUserContextUpsertResult> UpdateAssistantUserContext(AssistantUserContext assistantUserContext, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = new GatewayServiceClient(
               await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                   .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
               _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var newOpenAIAssistantId = default(string);
            var newOpenAIAssistantThreadId = default(string);
            var newOpenAIAssistantVectorStoreId = default(string);

            var incompleteConversations = assistantUserContext.Conversations.Values
                    .Where(c => string.IsNullOrWhiteSpace(c.OpenAIThreadId))
                    .ToList();

            if (incompleteConversations.Count != 1)
                throw new ResourceProviderException($"The Assistant user context {assistantUserContext.Name} contains an incorrect number of incomplete conversations (must be 1). This indicates an inconsistent approach in the resource management flow.");

            var resourceReference = await _resourceReferenceStore!.GetResourceReference(assistantUserContext.Name);

            if (resourceReference == null)
            {
                var assistantUserContextResourceReference = new AzureOpenAIResourceReference
                {
                    Name = assistantUserContext.Name!,
                    Type = assistantUserContext.Type!,
                    Filename = $"/{_name}/{assistantUserContext.Name}.json",
                    Deleted = false
                };

                #region Ensure that only one thread can create the resource at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingResourceReference = await _resourceReferenceStore!.GetResourceReference(assistantUserContext.Name);
                    if (existingResourceReference == null)
                    {
                        assistantUserContext.ObjectId = ResourcePath.GetObjectId(
                           _instanceSettings.Id,
                           _name,
                           AzureOpenAIResourceTypeNames.AssistantUserContexts,
                           assistantUserContext.Name);

                        // Always create the file user context associated with the assistant user context.
                        var newFileUserContextName = $"{assistantUserContext.UserPrincipalName.NormalizeUserPrincipalName()}-file-{_instanceSettings.Id.ToLower()}";

                        UpdateBaseProperties(assistantUserContext, userIdentity, isNew: true);

                        var existingFileUserContextReference = await _resourceReferenceStore!.GetResourceReference(newFileUserContextName);
                        if (existingFileUserContextReference == null)
                        {
                            var newFileUserContext = new FileUserContext()
                            {
                                UserPrincipalName = assistantUserContext.UserPrincipalName,
                                Endpoint = assistantUserContext.Endpoint,
                                Name = newFileUserContextName,
                                AssistantUserContextName = assistantUserContext.Name
                            };
                            var newUserFileContextResourceReference = new AzureOpenAIResourceReference
                            {
                                Name = newFileUserContextName,
                                Type = AzureOpenAITypes.FileUserContext,
                                Filename = $"/{_name}/{newFileUserContextName}.json",
                                Deleted = false
                            };

                            await CreateResources<AssistantUserContext, FileUserContext>(
                                assistantUserContextResourceReference, assistantUserContext,
                                newUserFileContextResourceReference, newFileUserContext);
                        }
                        else
                        {
                            await CreateResource<AssistantUserContext>(assistantUserContextResourceReference, assistantUserContext);
                        }
                    }
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion

                var result = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    assistantUserContextResourceReference.Name,
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

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject);
                newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();

                #region Ensure that only one thread can update the assistant user context at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingAssistantUserContext = await LoadResource<AssistantUserContext>(assistantUserContextResourceReference)
                        ?? throw new ResourceProviderException(
                            $"Could not load the {assistantUserContext.Name} assistant user context.");

                    existingAssistantUserContext.OpenAIAssistantId = newOpenAIAssistantId;
                    existingAssistantUserContext.OpenAIAssistantCreatedOn = DateTimeOffset.UtcNow;

                    var conversation = existingAssistantUserContext.Conversations.Values
                        .SingleOrDefault(c => string.IsNullOrWhiteSpace(c.OpenAIThreadId))
                        ?? throw new ResourceProviderException("Could not find a conversation with an empty assistant thread id.");

                    conversation.OpenAIThreadId = newOpenAIAssistantThreadId;
                    conversation.OpenAIThreadCreatedOn = assistantUserContext.OpenAIAssistantCreatedOn;
                    conversation.OpenAIVectorStoreId = newOpenAIAssistantVectorStoreId;

                    UpdateBaseProperties(existingAssistantUserContext, userIdentity, isNew: false);
                    await SaveResource<AssistantUserContext>(assistantUserContextResourceReference, existingAssistantUserContext);

                    return new AssistantUserContextUpsertResult
                    {
                        ObjectId = assistantUserContext.ObjectId,
                        ResourceExists = false,
                        NewOpenAIAssistantId = newOpenAIAssistantId,
                        NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId,
                        NewOpenAIAssistantVectorStoreId = newOpenAIAssistantVectorStoreId
                    };
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion
            }
            else
            {
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

                result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject);
                if (newOpenAIAssistantVectorStoreIdObject != null)
                {
                    newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();
                    incompleteConversations[0].OpenAIVectorStoreId = newOpenAIAssistantVectorStoreId;
                }

                incompleteConversations[0].OpenAIThreadId = newOpenAIAssistantThreadId;
                incompleteConversations[0].OpenAIThreadCreatedOn = DateTimeOffset.UtcNow;

                #region Ensure that only one thread can update the assistant user context at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingAssistantUserContext = await LoadResource<AssistantUserContext>(resourceReference)
                        ?? throw new ResourceProviderException(
                            $"Could not load the {resourceReference.Name} assistant user context.");

                    if (existingAssistantUserContext.Conversations.ContainsKey(incompleteConversations[0].FoundationaLLMSessionId))
                        throw new ResourceProviderException(
                            $"An OpenAI thread was already created for the FoundationaLLM session {incompleteConversations[0].FoundationaLLMSessionId}.",
                            StatusCodes.Status400BadRequest);

                    existingAssistantUserContext.Conversations.Add(
                        incompleteConversations[0].FoundationaLLMSessionId,
                        incompleteConversations[0]);

                    UpdateBaseProperties(existingAssistantUserContext, userIdentity, isNew: false);
                    await SaveResource<AssistantUserContext>(resourceReference, existingAssistantUserContext);

                    return new AssistantUserContextUpsertResult
                    {
                        ObjectId = existingAssistantUserContext.ObjectId,
                        ResourceExists = true,
                        NewOpenAIAssistantId = newOpenAIAssistantId,
                        NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId,
                        NewOpenAIAssistantVectorStoreId = newOpenAIAssistantVectorStoreId
                    };

                }
                finally
                {
                    _localLock.Release();
                }

                #endregion
            }
        }

        private async Task<FileUserContextUpsertResult> UpdateFileUserContext(FileUserContext fileUserContext, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = new GatewayServiceClient(
               await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                   .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
               _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var newOpenAIFileId = default(string);

            var incompleteFiles = fileUserContext.Files.Values
                    .Where(c => string.IsNullOrWhiteSpace(c.OpenAIFileId))
                    .ToList();

            if (incompleteFiles.Count > 1)
                throw new ResourceProviderException($"The File user context {fileUserContext.Name} contains an incorrect number of incomplete files (must be at most 1). This indicates an inconsistent approach in the resource management flow.");

            var resourceReference = await _resourceReferenceStore!.GetResourceReference(fileUserContext.Name);
            
            if (resourceReference == null)
            {
                var fileUserContextResourceReference = new AzureOpenAIResourceReference
                {
                    Name = fileUserContext.Name!,
                    Type = fileUserContext.Type!,
                    Filename = $"/{_name}/{fileUserContext.Name}.json",
                    Deleted = false
                };

                #region Ensure that only one thread can create the resource at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingResourceReference = await _resourceReferenceStore!.GetResourceReference(fileUserContext.Name);
                    if (existingResourceReference == null)
                    {
                        fileUserContext.ObjectId = ResourcePath.GetObjectId(
                            _instanceSettings.Id,
                            _name,
                            AzureOpenAIResourceTypeNames.FileUserContexts,
                            fileUserContext.Name);

                        // Always create the assistant user context associated with the file user context.
                        //var newAssistantUserContextName = $"{fileUserContext.UserPrincipalName.NormalizeUserPrincipalName()}-assistant-{_instanceSettings.Id.ToLower()}";
                        //var newAssistantUserContext = new AssistantUserContext()
                        //{
                        //    Name = newAssistantUserContextName,
                        //    UserPrincipalName = fileUserContext.UserPrincipalName,
                        //    Endpoint = "not_initialized",
                        //    ModelDeploymentName = "not_initialized",
                        //    Prompt = "not_initialized",
                        //};

                        //var newAssistantContextResourceReference = new AzureOpenAIResourceReference
                        //{
                        //    Name = newAssistantUserContextName,
                        //    Type = AzureOpenAITypes.AssistantUserContext,
                        //    Filename = $"/{_name}/{newAssistantUserContextName}.json",
                        //    Deleted = false
                        //};

                        UpdateBaseProperties(fileUserContext, userIdentity, isNew: true);
                        //await CreateResources<FileUserContext, AssistantUserContext>(
                        //    fileUserContextResourceReference, fileUserContext,
                        //    newAssistantContextResourceReference, newAssistantUserContext);
                        await CreateResource<FileUserContext>(fileUserContextResourceReference, fileUserContext);
                    }
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion

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
                            { OpenAIAgentCapabilityParameterNames.AttachmentObjectId,  incompleteFiles[0].FoundationaLLMObjectId }
                        });

                    result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantFileId, out var newOpenAIFileIdObject);
                    newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();
                }

                #region Ensure that only one thread can update the Files collection at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingFileUserContext = await LoadResource<FileUserContext>(fileUserContextResourceReference)
                        ?? throw new ResourceProviderException(
                            $"Could not load the {fileUserContext.Name} file user context.");

                    if (incompleteFiles.Count == 1)
                    {
                        if (existingFileUserContext.Files.TryGetValue(incompleteFiles[0].FoundationaLLMObjectId,
                                out var existingFileMapping))
                        {
                            existingFileMapping.OpenAIFileId = newOpenAIFileId;
                            existingFileMapping.OpenAIFileUploadedOn = DateTimeOffset.UtcNow;
                        }
                        else
                        {
                            incompleteFiles[0].OpenAIFileId = newOpenAIFileId;
                            incompleteFiles[0].OpenAIFileUploadedOn = DateTimeOffset.UtcNow;
                            existingFileUserContext.Files.Add(incompleteFiles[0].FoundationaLLMObjectId, incompleteFiles[0]);
                        }
                    }

                    // Merge the new file mappings into the existing file user context.
                    foreach (var mapping in fileUserContext.Files.Where(f =>
                        !existingFileUserContext.Files.ContainsKey(f.Key)))
                    {
                        existingFileUserContext.Files.Add(mapping.Key, mapping.Value);
                    }

                    UpdateBaseProperties(existingFileUserContext, userIdentity, isNew: false);
                    await SaveResource<FileUserContext>(fileUserContextResourceReference, existingFileUserContext);

                    return new FileUserContextUpsertResult
                    {
                        ObjectId = existingFileUserContext.ObjectId,
                        ResourceExists = false,
                        NewOpenAIFileId = newOpenAIFileId!
                    };
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion
            }
            else
            {
                if (incompleteFiles.Count == 1)
                {
                    var result = await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.OpenAIAssistants,
                        fileUserContext.AssistantUserContextName,
                        new()
                        {
                            { OpenAIAgentCapabilityParameterNames.CreateAssistantFile, true},
                            { OpenAIAgentCapabilityParameterNames.Endpoint, fileUserContext.Endpoint},
                            {
                                OpenAIAgentCapabilityParameterNames.AttachmentObjectId,
                                incompleteFiles[0].FoundationaLLMObjectId
                            }
                        });

                    result.TryGetValue(OpenAIAgentCapabilityParameterNames.AssistantFileId,
                        out var newOpenAIFileIdObject);
                    newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();
                }

                #region Ensure that only one thread can update the Files collection at a time.

                try
                {
                    await _localLock.WaitAsync();

                    var existingFileUserContext = await LoadResource<FileUserContext>(resourceReference)
                        ?? throw new ResourceProviderException(
                            $"Could not load the {resourceReference.Name} file user context.");

                    if (incompleteFiles.Count == 1)
                    {
                        if (existingFileUserContext.Files.ContainsKey(incompleteFiles[0].FoundationaLLMObjectId))
                            throw new ResourceProviderException(
                                $"An OpenAI file was already created for the FoundationaLLM attachment {incompleteFiles[0].FoundationaLLMObjectId}.",
                                StatusCodes.Status400BadRequest);

                        incompleteFiles[0].OpenAIFileId = newOpenAIFileId;
                        incompleteFiles[0].OpenAIFileUploadedOn = DateTimeOffset.UtcNow;

                        existingFileUserContext.Files.Add(
                            incompleteFiles[0].FoundationaLLMObjectId,
                            incompleteFiles[0]);
                    }

                    // Merge the new file mappings into the existing file user context.
                    foreach (var mapping in fileUserContext.Files.Where(f =>
                        !existingFileUserContext.Files.ContainsKey(f.Key)))
                    {
                        existingFileUserContext.Files.Add(mapping.Key, mapping.Value);
                    }

                    UpdateBaseProperties(existingFileUserContext, userIdentity, isNew: false);
                    await SaveResource<FileUserContext>(resourceReference, existingFileUserContext);

                    return new FileUserContextUpsertResult
                    {
                        ObjectId = existingFileUserContext.ObjectId,
                        ResourceExists = true,
                        NewOpenAIFileId = newOpenAIFileId!
                    };
                }
                finally
                {
                    _localLock.Release();
                }

                #endregion
            }
        }

        #endregion
    }
}
