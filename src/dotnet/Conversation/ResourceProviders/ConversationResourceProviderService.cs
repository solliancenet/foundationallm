using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using ConversationModels = FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Conversation.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Conversation resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ConversationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILogger<ConversationResourceProviderService> logger)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
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

        /// <inheritdoc />
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ConversationResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_Conversation;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        // This resource provider does not support the Management API.

        #endregion

        #region Resource provider strongly typed operations

        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            var policyDefinition = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            var conversationType = options?.Parameters?.GetValueOrDefault(ConversationResourceProviderGetParameterNames.ConversationType) as string
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider requires the {ConversationResourceProviderGetParameterNames.ConversationType} parameter to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resources.
            // The implementation of the PEP is straightforward: the resource provider loads the resources from the data store matching the UPN of the user identity.

            var result = await _cosmosDBService.GetConversationsAsync(
                conversationType,
                userIdentity.UPN!);

            return result.Select(r => new ResourceProviderGetResult<ConversationModels.Conversation>
            {
                Resource = r,
                Actions = [],
                Roles = []
            }).ToList();
        }

        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resource.
            // The implementation of the PEP is straightforward: the resource provider loads the resource and validates the UPN of the user identity.
            // TODO: improve the performance of the GetSessionAsync method by using a more efficient query.

            var result = await _cosmosDBService.GetConversationAsync(resourcePath.ResourceId!)
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource.",
                    StatusCodes.Status404NotFound);

            if (!StringComparer.Ordinal.Equals(result!.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            return (result as T)!;
        }

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

            var existingConversation = await _cosmosDBService.GetConversationAsync(resourcePath.ResourceId!);

            if (existingConversation != null
                && !StringComparer.Ordinal.Equals(existingConversation.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            var updatedConversation = (resource as ConversationModels.Conversation)!;

            if (!StringComparer.Ordinal.Equals(updatedConversation!.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to use the provided resource to update the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            UpdateBaseProperties(updatedConversation, userIdentity, existingConversation == null);

            if (existingConversation == null
                || string.IsNullOrWhiteSpace(updatedConversation.ObjectId))
                updatedConversation.ObjectId = resourcePath.RawResourcePath;

            var conversation = await _cosmosDBService.CreateOrUpdateConversationAsync(updatedConversation)
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource.",
                    StatusCodes.Status404NotFound);

            return (new ResourceProviderUpsertResult<T>
            {
                ObjectId = updatedConversation.ObjectId,
                ResourceExists = existingConversation != null,
                Resource = conversation as T
            } as TResult)!;
        }

        protected override async Task<TResult> UpdateResourcePropertiesAsyncInternal<T, TResult>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, Dictionary<string, object?> propertyValues, UnifiedUserIdentity userIdentity)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to update the resource properties.
            // The implementation relies on using a filter predicate to ensure that the user identity is authorized to update the resource properties.

            var result = await _cosmosDBService.PatchConversationPropertiesAsync(
                resourcePath.ResourceId!,
                userIdentity.UPN!,
                propertyValues)
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource. "
                    + "This indicates that either the resource does not exist or existing policies do not allow the user to update it.",
                    StatusCodes.Status404NotFound);

            return (new ResourceProviderUpsertResult<ConversationModels.Conversation>
            {
                ObjectId = resourcePath.RawResourcePath,
                ResourceExists = true,
                Resource = result
            } as TResult)!;
        }

        protected override async Task DeleteResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to logically delete the resource.
            // The implementation relies on attempting to load the resource and validate the user identity is authorized to delete it.

            var existingConversation = await _cosmosDBService.GetConversationAsync(resourcePath.ResourceId!)
                ?? throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource.",
                    StatusCodes.Status404NotFound);

            if (!StringComparer.Ordinal.Equals(existingConversation.UPN, userIdentity.UPN))
                throw new ResourceProviderException(
                    $"The user {userIdentity.UPN} is not authorized to access the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status403Forbidden);

            await _cosmosDBService.DeleteConversationAsync(resourcePath.ResourceId!);
        }

        #endregion

    }
}
