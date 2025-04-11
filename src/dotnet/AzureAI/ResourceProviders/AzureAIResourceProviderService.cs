using Azure.AI.Projects;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.AzureAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzureAI.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AzureAI resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>    
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureAIResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,        
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILogger<AzureAIResourceProviderService> logger)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            new NullStorageService(),
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            eventTypesToSubscribe: null,
            useInternalReferencesStore: false)
    {
        
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AzureOpenAIResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AzureAI;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        // This resource provider does not support the Management API.

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resource.
            // The implementation of the PEP is straightforward: the resource provider loads the resource and validates the UPN of the user identity.

            var result = await _cosmosDBService.GetItemAsync<T>(
                AzureCosmosDBContainers.ExternalResources,
                resourcePath.MainResourceId!,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}")
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource.",
                    StatusCodes.Status404NotFound);

            if (!StringComparer.Ordinal.Equals((result as AzureCosmosDBResource)!.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            return result;
        }

        /// <inheritdoc/>
        protected override async Task<(bool Exists, bool Deleted)> ResourceExistsAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resource.
            // The implementation of the PEP is straightforward: the resource provider checks for the existence of the resource considering
            // only resources that are matching the UPN of the user identity.

            var result = await _cosmosDBService.GetItemAsync<T>(
                AzureCosmosDBContainers.ExternalResources,
                resourcePath.MainResourceId!,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}");

            return (result != null, result?.Deleted ?? false);
        }

        /// <inheritdoc/>
        protected override async Task<TResult> ExecuteResourceActionAsyncInternal<T, TAction, TResult>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, TAction actionPayload, UnifiedUserIdentity userIdentity)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resource.
            // The implementation of the PEP is straightforward: the resource provider loads the resource and validates the UPN of the user identity.

            var result = await _cosmosDBService.GetItemAsync<T>(
                AzureCosmosDBContainers.ExternalResources,
                resourcePath.MainResourceId!,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}")
                ?? throw new ResourceProviderException(
                        $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource.",
                        StatusCodes.Status404NotFound);

            if (!StringComparer.Ordinal.Equals((result as AzureCosmosDBResource)!.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            return resourcePath.Action switch
            {
                ResourceProviderActions.LoadFileContent => ((await LoadFileContentAsync((result as AzureAIAgentFileMapping)!)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
        }

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to upsert the resource.
            // The implementation of the PEP is straightforward: the resource provider validates the UPN of the user identity.

            var updatedResource = (resource as AzureAIAgentResourceBase)!;

            if (!StringComparer.Ordinal.Equals(updatedResource.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to use the provided resource to update the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            var existingResource = default(T);
            if (!string.IsNullOrWhiteSpace(resourcePath.MainResourceId))
            {
                existingResource = await _cosmosDBService.GetItemAsync<T>(
                    AzureCosmosDBContainers.ExternalResources,
                    resourcePath.MainResourceId!,
                    $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}");
            }

            if (existingResource != null)
            {
                if (!StringComparer.Ordinal.Equals((existingResource as AzureAIAgentResourceBase)!.UPN,
                        userIdentity.UPN))
                {
                    throw new ResourceProviderException(
                        $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                        StatusCodes.Status403Forbidden);
                }

                if (existingResource is AzureAIAgentResourceBase resource1
                    && updatedResource is AzureAIAgentResourceBase resource2
                    && (
                        !StringComparer.Ordinal.Equals(resource1.Id, resource2.Id)
                        || !StringComparer.Ordinal.Equals(resource1.Name, resource2.Name)
                        || !StringComparer.Ordinal.Equals(resource1.Type, resource2.Type)
                        || !StringComparer.Ordinal.Equals(resource1.InstanceId, resource2.InstanceId)
                        || !StringComparer.Ordinal.Equals(resource1.PartitionKey, resource2.PartitionKey)
                    ))
                    throw new ResourceProviderException(
                        $"Updating one or more properties is not allowed because their values are immutable.",
                        StatusCodes.Status400BadRequest);
            }

            return updatedResource switch
            {
                AzureAIAgentConversationMapping conversationMapping => ((await UpdateConversationMappingAsync(
                    conversationMapping,
                    existingResource == null,
                    userIdentity,
                    options)) as TResult)!,
                AzureAIAgentFileMapping fileMapping => ((await UpdateFileMappingAsync(
                    fileMapping,
                    existingResource == null,
                    userIdentity,
                    options)) as TResult)!,
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };
        }

        #endregion

        #region Resource management

        private async Task<ResourceProviderActionResult<FileContent>> LoadFileContentAsync(AzureAIAgentFileMapping fileMapping)
        {           
            var agentsClient = new AgentsClient(fileMapping.ProjectConnectionString, ServiceContext.AzureCredential);

            // Retrieve the file content from the Azure AI Agent service using the file ID.
            var result = await agentsClient.GetFileContentAsync(fileMapping.AzureAIAgentFileId);                     

            return new ResourceProviderActionResult<FileContent>(fileMapping.FileObjectId, true)
            {
                Resource = new()
                {
                    Name = fileMapping.AzureAIAgentFileId,
                    OriginalFileName = fileMapping.OriginalFileName,
                    ContentType = fileMapping.FileContentType,
                    BinaryContent = result.Value.ToMemory()
                }
            };
        }

        private async Task<AzureAIAgentConversationMappingUpsertResult> UpdateConversationMappingAsync(
            AzureAIAgentConversationMapping conversationMapping,
            bool isNew,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            #region Load and validate upsert options

            var agentObjectId = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.AgentObjectId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureAIResourceProviderUpsertParameterNames.AgentObjectId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var conversationId = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.ConversationId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureAIResourceProviderUpsertParameterNames.ConversationId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var azureAIAgentId = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.AzureAIAgentId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureAIResourceProviderUpsertParameterNames.AzureAIAgentId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var mustCreateAgentThread = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentThread) as bool?
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentThread} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            #endregion

            #region Create the Azure AI Agent Service thread

            var newAzureAIAgentThreadId = default(string);
            var newAzureAIAgentVectorStoreId = default(string);

            if (isNew)
            {
                conversationMapping.ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name,
                    AzureAIResourceTypeNames.AgentConversationMappings, conversationMapping.Name);
            }

            if (mustCreateAgentThread)
            {
                var gatewayClient = new GatewayServiceClient(
                   await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                       .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                   _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

                Dictionary<string, object> parameters = new()
                {
                    { AzureAIAgentServiceCapabilityParameterNames.AgentId, azureAIAgentId },
                    { AzureAIAgentServiceCapabilityParameterNames.CreateThread, mustCreateAgentThread },
                    { AzureAIAgentServiceCapabilityParameterNames.ProjectConnectionString, conversationMapping.ProjectConnectionString }
                };

                var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.AzureAIAgents,
                    string.Empty,
                    parameters);

                var referenceTime = DateTime.UtcNow;

                if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.ThreadId, out var newAzureAIAgentThreadIdObject)
                    && newAzureAIAgentThreadIdObject != null)
                    newAzureAIAgentThreadId = ((JsonElement)newAzureAIAgentThreadIdObject!).Deserialize<string>();

                if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.VectorStoreId, out var newAzureAIAgentVectorStoreIdObject)
                    && newAzureAIAgentVectorStoreIdObject != null)
                    newAzureAIAgentVectorStoreId = ((JsonElement)newAzureAIAgentVectorStoreIdObject!).Deserialize<string>();

                if (string.IsNullOrWhiteSpace(newAzureAIAgentVectorStoreId))
                    throw new ResourceProviderException(
                        $"The Azure AI Agent Service vector store was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                if (string.IsNullOrWhiteSpace(newAzureAIAgentThreadId))
                    throw new ResourceProviderException(
                        $"The Azure AI Agent Service thread was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                conversationMapping.AzureAIAgentThreadId = newAzureAIAgentThreadId;
                conversationMapping.AzureAIAgentThreadCreatedOn = referenceTime;
                conversationMapping.AzureAIAgentVectorStoreId = newAzureAIAgentVectorStoreId;
                conversationMapping.AzureAIAgentVectorStoreCreatedOn = referenceTime;
            }

            #endregion

            UpdateBaseProperties(conversationMapping, userIdentity, isNew: isNew);

            await _cosmosDBService.UpsertItemAsync<AzureAIAgentConversationMapping>(
                AzureCosmosDBContainers.ExternalResources,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}",
                conversationMapping);

            return new AzureAIAgentConversationMappingUpsertResult
            {
                ObjectId = conversationMapping.ObjectId!,
                ResourceExists = !isNew,
                NewAzureAIAgentThreadId = newAzureAIAgentThreadId,
                NewAzureAIAgentVectorStoreId = newAzureAIAgentVectorStoreId
            };
        }

        private async Task<ResourceProviderUpsertResult<AzureAIAgentFileMapping>> UpdateFileMappingAsync(
            AzureAIAgentFileMapping fileMapping,
            bool isNew,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            #region Load and validate upsert options

            var agentObjectId = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.AgentObjectId) as string;

            var attachmentObjectId = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.AttachmentObjectId) as string;

            var mustCreateAzureAIAgentFile = options?.Parameters.GetValueOrDefault(AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentFile) as bool?
                ?? false;

            #endregion

            #region Create the OpenAI file

            var newAzureAIAgentFileId = default(string);

            if (mustCreateAzureAIAgentFile)
            {
                var gatewayClient = new GatewayServiceClient(
                   await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                       .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                   _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

                Dictionary<string, object> parameters = new()
                    {
                        { AzureAIAgentServiceCapabilityParameterNames.CreateFile, true },
                        { AzureAIAgentServiceCapabilityParameterNames.ProjectConnectionString, fileMapping.ProjectConnectionString },
                        { AzureAIAgentServiceCapabilityParameterNames.AttachmentObjectId,  attachmentObjectId! }
                    };

                var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.AzureAIAgents,
                    string.Empty,
                    parameters);

                var referenceTime = DateTime.UtcNow;

                if (agentCapabilityResult.TryGetValue(AzureAIAgentServiceCapabilityParameterNames.FileId, out var newAzureAIAgentFileIdObject)
                    && newAzureAIAgentFileIdObject != null)
                    newAzureAIAgentFileId = ((JsonElement)newAzureAIAgentFileIdObject!).Deserialize<string>();

                if (string.IsNullOrWhiteSpace(newAzureAIAgentFileId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant file was not created for the agent {agentObjectId}.",
                        StatusCodes.Status500InternalServerError);

                fileMapping.Id = newAzureAIAgentFileId;
                fileMapping.Name = newAzureAIAgentFileId;
                fileMapping.ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name,
                    AzureOpenAIResourceTypeNames.FileMappings, newAzureAIAgentFileId);
                fileMapping.AzureAIAgentFileId = newAzureAIAgentFileId;
                fileMapping.AzureAIAgentFileUploadedOn = referenceTime;
            }

            #endregion

            UpdateBaseProperties(fileMapping, userIdentity, isNew: isNew);

            await _cosmosDBService.UpsertItemAsync<AzureAIAgentFileMapping>(
                AzureCosmosDBContainers.ExternalResources,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}",
                fileMapping);

            return new ResourceProviderUpsertResult<AzureAIAgentFileMapping>
            {
                ObjectId = fileMapping.ObjectId!,
                ResourceExists = isNew,
                Resource = fileMapping
            };
        }

        #endregion
    }
}
