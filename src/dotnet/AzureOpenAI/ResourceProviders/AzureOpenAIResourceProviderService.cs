using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes.Models;
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
using Microsoft.Graph.Drives.Item.Items.Item.Workbook.Functions.False;
using System.Text.Json;

namespace FoundationaLLM.AzureOpenAI.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class AzureOpenAIResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AzureOpenAI)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILogger<AzureOpenAIResourceProviderService> logger)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
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
        protected override string _name => ResourceProviderNames.FoundationaLLM_AzureOpenAI;

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
                ResourceProviderActions.LoadFileContent => ((await LoadFileContent((result as AzureOpenAIFileMapping)!)) as TResult)!,
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

            var updatedResource = (resource as AzureOpenAIResourceBase)!;

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
                if (!StringComparer.Ordinal.Equals((existingResource as AzureOpenAIResourceBase)!.UPN,
                        userIdentity.UPN))
                {
                    throw new ResourceProviderException(
                        $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                        StatusCodes.Status403Forbidden);
                }

                if (existingResource is AzureOpenAIResourceBase resource1
                    && updatedResource is AzureOpenAIResourceBase resource2
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
                AzureOpenAIConversationMapping conversationMapping => ((await UpdateConversationMapping(
                    conversationMapping,
                    existingResource == null,
                    userIdentity,
                    options)) as TResult)!,
                AzureOpenAIFileMapping fileMapping => ((await UpdateFileMapping(
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

        private async Task<ResourceProviderActionResult<FileContent>> LoadFileContent(AzureOpenAIFileMapping fileMapping)
        {
            var azureOpenAIClient = new AzureOpenAIClient(new Uri(fileMapping.OpenAIEndpoint), DefaultAuthentication.AzureCredential);
            var fileClient = azureOpenAIClient.GetOpenAIFileClient();

            // Retrieve using the OpenAI file ID.           
            var result = await fileClient.DownloadFileAsync(fileMapping!.OpenAIFileId);

            return new ResourceProviderActionResult<FileContent>(true)
            {
                Resource = new()
                {
                    Name = fileMapping!.OpenAIFileId!,
                    OriginalFileName = fileMapping!.OriginalFileName,
                    ContentType = fileMapping.FileContentType,
                    BinaryContent = result.Value.ToMemory()
                }
            };
        }

        private async Task<AzureOpenAIConversationMappingUpsertResult> UpdateConversationMapping(
            AzureOpenAIConversationMapping conversationMapping,
            bool isNew,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            #region Load and validate upsert options

            var agentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var conversationId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.ConversationId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.ConversationId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var openAIAssistantId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.OpenAIAssistantId) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.OpenAIAssistantId} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            var mustCreateAssistantThread = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread) as bool?
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread} parameter to update the {conversationMapping.Name} conversation mapping.",
                    StatusCodes.Status400BadRequest);

            #endregion

            #region Create the OpenAI assistant thread

            var newOpenAIAssistantThreadId = default(string);
            var newOpenAIVectorStoreId = default(string);

            if (isNew)
            {
                conversationMapping.ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name,
                    AzureOpenAIResourceTypeNames.ConversationMappings, conversationMapping.Name);
            }

            if (mustCreateAssistantThread)
            {
                var gatewayClient = new GatewayServiceClient(
                   await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                       .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                   _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

                Dictionary<string, object> parameters = new()
                {
                    { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, openAIAssistantId },
                    { OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistantThread, mustCreateAssistantThread },
                    { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, conversationMapping.OpenAIEndpoint }
                };

                var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    string.Empty,
                    parameters);

                var referenceTime = DateTime.UtcNow;

                if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIAssistantThreadId, out var newOpenAIAssistantThreadIdObject)
                    && newOpenAIAssistantThreadIdObject != null)
                    newOpenAIAssistantThreadId = ((JsonElement)newOpenAIAssistantThreadIdObject!).Deserialize<string>();

                if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                    && newOpenAIAssistantVectorStoreIdObject != null)
                    newOpenAIVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();

                if (string.IsNullOrWhiteSpace(newOpenAIVectorStoreId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant vector store was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                if (string.IsNullOrWhiteSpace(newOpenAIAssistantThreadId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant thread was not created for the agent {agentObjectId} and conversation {conversationId}.",
                        StatusCodes.Status500InternalServerError);

                conversationMapping.OpenAIAssistantsThreadId = newOpenAIAssistantThreadId;
                conversationMapping.OpenAIAssistantsThreadCreatedOn = referenceTime;
                conversationMapping.OpenAIVectorStoreId = newOpenAIVectorStoreId;
                conversationMapping.OpenAIVectorStoreCreatedOn = referenceTime;
            }

            #endregion

            UpdateBaseProperties(conversationMapping, userIdentity, isNew: isNew);

            await _cosmosDBService.UpsertItemAsync<AzureOpenAIConversationMapping>(
                AzureCosmosDBContainers.ExternalResources,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}",
                conversationMapping);

            return new AzureOpenAIConversationMappingUpsertResult
            {
                ObjectId = conversationMapping.ObjectId!,
                ResourceExists = !isNew,
                NewOpenAIAssistantThreadId = newOpenAIAssistantThreadId!,
                NewOpenAIVectorStoreId = newOpenAIVectorStoreId
            };
        }

        private async Task<ResourceProviderUpsertResult<AzureOpenAIFileMapping>> UpdateFileMapping(
            AzureOpenAIFileMapping fileMapping,
            bool isNew,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
        {
            #region Load and validate upsert options

            var agentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId) as string;

            var attachmentObjectId = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.AttachmentObjectId) as string;

            var mustCreateOpenAIFile = options?.Parameters.GetValueOrDefault(AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIFile) as bool?
                ?? false;

            #endregion

            #region Create the OpenAI file

            var newOpenAIFileId = default(string);

            if (mustCreateOpenAIFile)
            {
                var gatewayClient = new GatewayServiceClient(
                   await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                       .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                   _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

                Dictionary<string, object> parameters = new()
                    {
                        { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, true },
                        { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, fileMapping.OpenAIEndpoint },
                        { OpenAIAgentCapabilityParameterNames.AttachmentObjectId,  attachmentObjectId }
                    };

                var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                    _instanceSettings.Id,
                    AgentCapabilityCategoryNames.OpenAIAssistants,
                    string.Empty,
                    parameters);

                var referenceTime = DateTime.UtcNow;

                if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileId, out var newOpenAIFileIdObject)
                    && newOpenAIFileIdObject != null)
                        newOpenAIFileId = ((JsonElement)newOpenAIFileIdObject!).Deserialize<string>();

                if (string.IsNullOrWhiteSpace(newOpenAIFileId))
                    throw new ResourceProviderException(
                        $"The OpenAI assistant file was not created for the agent {agentObjectId}.",
                        StatusCodes.Status500InternalServerError);

                fileMapping.Id = newOpenAIFileId;
                fileMapping.Name = newOpenAIFileId;
                fileMapping.ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name,
                    AzureOpenAIResourceTypeNames.FileMappings, newOpenAIFileId);
                fileMapping.OpenAIFileId = newOpenAIFileId;
                fileMapping.OpenAIFileUploadedOn = referenceTime;
            }

            #endregion

            UpdateBaseProperties(fileMapping, userIdentity, isNew: isNew);

            await _cosmosDBService.UpsertItemAsync<AzureOpenAIFileMapping>(
                AzureCosmosDBContainers.ExternalResources,
                $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-{_instanceSettings.Id}",
                fileMapping);

            return new ResourceProviderUpsertResult<AzureOpenAIFileMapping>
            {
                ObjectId = fileMapping.ObjectId!,
                ResourceExists = isNew,
                Resource = fileMapping
            };
        }

        #endregion
    }
}
