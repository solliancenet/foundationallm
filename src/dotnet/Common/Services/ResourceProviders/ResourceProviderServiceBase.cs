using Azure.Messaging;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.Cache;
using FoundationaLLM.Common.Services.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Implements basic resource provider functionality
    /// </summary>
    /// <typeparam name="TResourceReference">The type of the resource reference used by the resource provider.</typeparam>
    public class ResourceProviderServiceBase<TResourceReference> : IResourceProviderService
        where TResourceReference : ResourceReference
    {
        private bool _isInitialized = false;

        private LocalEventService? _localEventService;
        private readonly List<string>? _eventTypesToSubscribe;
        private readonly ImmutableList<string> _allowedResourceProviders;
        private readonly Dictionary<string, ResourceTypeDescriptor> _allowedResourceTypes;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviders = [];

        private readonly bool _useInternalReferencesStore;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly ResourceProviderCacheSettings _cacheSettings;
        private readonly IResourceProviderResourceCacheService? _resourceCache;
        private const string CACHE_WARMUP_FILE_NAME = "_cache_warmup.json";

        /// <summary>
        /// The resource reference store used by the resource provider.
        /// </summary>
        protected ResourceProviderResourceReferenceStore<TResourceReference>? _resourceReferenceStore;

        /// <summary>
        /// The <see cref="IServiceProvider"/> tha provides dependency injection services.
        /// </summary>
        protected readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The <see cref="IAuthorizationServiceClient"/> providing authorization services to the resource provider.
        /// </summary>
        protected readonly IAuthorizationServiceClient _authorizationServiceClient;

        /// <summary>
        /// The <see cref="IStorageService"/> providing storage services to the resource provider.
        /// </summary>
        protected readonly IStorageService _storageService;

        /// <summary>
        /// The <see cref="IEventService"/> providing event services to the resource provider.
        /// </summary>
        protected readonly IEventService _eventService;

        /// <summary>
        /// The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.
        /// </summary>
        protected readonly IResourceValidatorFactory _resourceValidatorFactory;

        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The <see cref="InstanceSettings"/> that provides instance-wide settings.
        /// </summary>
        protected readonly InstanceSettings _instanceSettings;

        /// <summary>
        /// The name of the storage container name used by the resource provider to store its internal data.
        /// </summary>
        protected virtual string _storageContainerName => "resource-provider";

        /// <summary>
        /// The name of the resource provider. Must be overridden in derived classes.
        /// </summary>
        protected virtual string _name => throw new NotImplementedException();

        /// <summary>
        /// Default JSON serialization settings.
        /// </summary>
        protected virtual JsonSerializerOptions _serializerSettings => new()
        {
            WriteIndented = true
        };

        /// <inheritdoc/>
        public string Name => _name;

        /// <inheritdoc/>
        public bool IsInitialized  => _isInitialized;

        /// <inheritdoc/>
        public Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => _allowedResourceTypes;

        /// <inheritdoc/>
        public string StorageAccountName => _storageService.StorageAccountName;

        /// <inheritdoc/>
        public string StorageContainerName => _storageContainerName;

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="instanceSettings">The <see cref="InstanceSettings"/> that provides instance-wide settings.</param>
        /// <param name="cacheSettings">The <see cref="ResourceProviderCacheSettings"/> that provides settings for the resource provider cache.</param>
        /// <param name="authorizationServiceClient">The <see cref="IAuthorizationServiceClient"/> providing authorization services to the resource provider.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the resource provider.</param>
        /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
        /// <param name="eventTypesToSubscribe">The list of Event Service event namespaces to subscribe to for local event processing.</param>
        /// <param name="useInternalReferencesStore">Indicates whether the resource provider should use the internal resource references store or provide one of its own.</param>
        public ResourceProviderServiceBase(
            InstanceSettings instanceSettings,
            ResourceProviderCacheSettings cacheSettings,
            IAuthorizationServiceClient authorizationServiceClient,
            IStorageService storageService,
            IEventService eventService,
            IResourceValidatorFactory resourceValidatorFactory,
            IServiceProvider serviceProvider,
            ILogger logger,
            List<string>? eventTypesToSubscribe = default,
            bool useInternalReferencesStore = false)
        {
            _authorizationServiceClient = authorizationServiceClient;
            _storageService = storageService;
            _eventService = eventService;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _instanceSettings = instanceSettings;
            _cacheSettings = cacheSettings;
            _eventTypesToSubscribe = eventTypesToSubscribe;
            _useInternalReferencesStore = useInternalReferencesStore;

            logger.LogInformation("Resource provider caching {CacheStatusString} enabled.",
                _cacheSettings.EnableCache ? "is" : "is not");

            if (_cacheSettings.EnableCache)
                _resourceCache = new ResourceProviderResourceCacheService(
                    _cacheSettings,
                    _logger);

            _allowedResourceProviders = [_name];
            _allowedResourceTypes = GetResourceTypes();

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        #region Initialization

        /// <inheritdoc/>
        public async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

                if (_useInternalReferencesStore)
                {
                    // The resource provider uses the default internal resource reference store.
                    _resourceReferenceStore = new ResourceProviderResourceReferenceStore<TResourceReference>(
                        this,
                        _storageService,
                        _logger);

                    await _resourceReferenceStore.LoadResourceReferences();
                }

                await InitializeInternal();

                if (_eventTypesToSubscribe != null
                    && _eventTypesToSubscribe.Count > 0)
                {
                    _localEventService = new LocalEventService(
                        new LocalEventServiceSettings { EventProcessingCycleSeconds = 10 },
                        _eventService,
                        _logger);
                    _localEventService.SubscribeToEventTypes(_eventTypesToSubscribe);
                    _localEventService.StartLocalEventProcessing(HandleEvents);
                }
                
                _isInitialized = true;

                if (_useInternalReferencesStore && _cacheSettings.EnableCache)
                    await WarmupCache();

                _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The {ResourceProviderName} resource provider failed to initialize.", _name);
            }
        }

        /// <inheritdoc/>
        public async Task WaitForInitialization()
        {
            if (IsInitialized)
                return;

            for (int i = 0; i < 6; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                if (IsInitialized)
                    return;
            }

            throw new ResourceProviderException($"The resource provider {Name} did not initialize within the expected time frame.");
        }

        private async Task WarmupCache()
        {
            try
            {
                if (!_cacheSettings.EnableCache)
                    return;

                _logger.LogInformation("Starting to warm up the cache for the {ResourceProvider} resource provider...", _name);

                var cacheWarmupFileName = $"/{_name}/{CACHE_WARMUP_FILE_NAME}";

                if (await _storageService.FileExistsAsync(
                    _storageContainerName,
                    cacheWarmupFileName,
                    default))
                {
                    var fileContent = await _storageService.ReadFileAsync(
                        _storageContainerName,
                        cacheWarmupFileName,
                        default);
                    var cacheWarmupConfigurations = JsonSerializer.Deserialize<List<ResourceProviderCacheWarmupConfiguration>>(
                        Encoding.UTF8.GetString(fileContent.ToArray()))!;

                    foreach (var cacheWarmupConfiguration in cacheWarmupConfigurations.Where(cwc => StringComparer.Ordinal.Equals(cwc.ServiceName, ServiceContext.ServiceName)))
                    foreach (var securityPrincipalId in cacheWarmupConfiguration.SecurityPrincipalIds)
                    {
                        var userIdentity = new UnifiedUserIdentity
                        {
                            UserId = securityPrincipalId,
                            Name = securityPrincipalId,
                            Username = securityPrincipalId,
                            UPN = securityPrincipalId,
                        };

                        foreach (var resourceObjectId in cacheWarmupConfiguration.ResourceObjectIds)
                        {
                            _logger.LogInformation("Loading object {ResourceObjectId} for security principal {SecurityPrincipalId}...", resourceObjectId, securityPrincipalId);
                            await HandleGetAsync(resourceObjectId, userIdentity);
                        }
                    }
                }
                else
                    _logger.LogInformation("The {ResourceProvider} resource provider does not have a cache warmup file.", _name);

                _logger.LogInformation("The cache for the {ResourceProvider} resource provider was successfully warmed up.", _name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while warming up the cache for the {ResourceProvider} resource provider.", _name);
            }
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of Initialize. Must be overridden in derived classes.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InitializeInternal()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the details about the resource types managed by the resource provider.
        /// </summary>
        /// <returns>A dictionary of <see cref="ResourceTypeDescriptor"/> objects with details about the resource types.</returns>
        protected virtual Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() => [];

        #endregion

        #endregion

        #region IManagementProviderService

        /// <inheritdoc/>
        public async Task<object> HandleGetAsync(string resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = ParseAndValidateResourcePath(resourcePath, HttpMethod.Get, false, requireResource: false);

            // Authorize access to the resource path.
            var authorizationResult = ParsedResourcePath.IsResourceTypePath
                ? await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, true, options?.IncludeRoles ?? false, options?.IncludeActions ?? false)
                : await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, options?.IncludeRoles ?? false, options?.IncludeActions ?? false);
           
            return await GetResourcesAsync(ParsedResourcePath, authorizationResult, userIdentity, options);
        }

        /// <inheritdoc/>
        public async Task<object> HandlePostAsync(string resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity)
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = ParseAndValidateResourcePath(resourcePath, HttpMethod.Post, true, requireResource: false);

            if (ParsedResourcePath.HasAction)
            {
                // Handle the action.
                if (string.IsNullOrWhiteSpace(serializedResource) || formFile != null)
                    throw new ResourceProviderException(
                        "Empty payloads or attached files are not allowed on resource provider action requests.",
                        StatusCodes.Status400BadRequest);

                // Some actions require a resource identifier.
                if (ParsedResourcePath.Action! == ResourceProviderActions.Purge
                    && !ParsedResourcePath.HasResourceId)
                    throw new ResourceProviderException(
                        $"The resource path {resourcePath} is required to have a resource identifier but none was found.",
                        StatusCodes.Status400BadRequest);

                // Authorize access to the resource path.
                // In the special case of the filter action, if the resource type path is not directly authorized,
                // the subordinate authorized resource paths must be expanded (and the overrides for ExecuteActionAsync must handle this).
                var actionAuthorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation,
                    ParsedResourcePath.Action! == ResourceProviderActions.Filter, false, false);

                return await ExecuteActionAsync(ParsedResourcePath, actionAuthorizationResult, serializedResource!, userIdentity);
            }

            // All resource upserts require a resource identifier.
            if (!ParsedResourcePath.HasResourceId)
                throw new ResourceProviderException(
                    $"The resource path {resourcePath} is required to have a resource identifier but none was found.",
                    StatusCodes.Status400BadRequest);

            // Authorize access to the resource path.
            var authorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            var upsertResult = await UpsertResourceAsync(ParsedResourcePath, serializedResource, formFile, userIdentity);

            await UpsertResourcePostProcess(ParsedResourcePath, (upsertResult as ResourceProviderUpsertResult)!, authorizationResult, userIdentity);

            await SendResourceProviderEvent(EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);

            return upsertResult;
        }

        /// <inheritdoc/>
        public async Task HandleDeleteAsync(string resourcePath, UnifiedUserIdentity userIdentity)
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = ParseAndValidateResourcePath(resourcePath, HttpMethod.Delete, false);

            // Authorize access to the resource path.
            await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            await DeleteResourceAsync(ParsedResourcePath, userIdentity);

            await SendResourceProviderEvent(EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <returns></returns>
        /// <remarks>
        /// The override implementation should return a list of resources or a single resource, depending on the resource path.
        /// It also must handle the authorization result and return the appropriate response as follows:
        /// <list type="number">
        /// <item>The resource path refers to a single resource. In this case, the authorization is already confirmed and
        /// the specific resource should be returned.</item>
        /// <item>The resource path refers to a resource type and the read action is authorized for the resource path itself.
        /// In this case, all resources must be returned according to the PBAC policies specified by the authorization result (if any).</item>
        /// <item>The resource path refers to a resource type and the read action is denied for the resource path itself.
        /// In this case, only the resources specified in the subordinate authorized resource paths list of the authorization result should be returned (if any).</item>
        /// </list>
        /// </remarks>
        protected virtual async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="serializedResource">The optional serialized resource being created or updated.</param>
        /// <param name="formFile">The optional file attached to the request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        protected virtual async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of ExecuteActionAsync. Must be overriden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result
        /// of the resource path authorization request.</param>
        /// <param name="serializedAction">The serialized details of the action being executed.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        /// <remarks>
        /// In the special case of the <c>filter</c> action, the override must handle the authorization result and return
        /// the appropriate response as follows:
        /// <list type="number">
        /// <item>The read action is authorized for the resource path itself.
        /// In this case, all matching resources must be returned according to the PBAC policies specified by the authorization result (if any).</item>
        /// <item>The read action is denied for the resource path itself.
        /// In this case, only the matching resources specified in the subordinate authorized resource paths list
        /// of the authorization result should be returned (if any).</item>
        /// </list>
        /// </remarks>
        protected virtual async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IResourceProviderService

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<T>>> GetResourcesAsync<T>(string instanceId, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
            where T : ResourceBase
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) =
                CreateAndValidateResourcePath(instanceId, HttpMethod.Get, typeof(T));

            var authorizationResult =
               await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, true, options?.IncludeRoles ?? false, options?.IncludeActions ?? false);

            return ((await GetResourcesAsync(ParsedResourcePath, authorizationResult, userIdentity, options)) as List<ResourceProviderGetResult<T>>)!;
        }

        /// <inheritdoc/>
        public async Task<T> GetResourceAsync<T>(string resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
            where T : ResourceBase
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) =
                ParseAndValidateResourcePath(resourcePath, HttpMethod.Get, false, typeof(T));

            // Authorize access to the resource path.
            var authorizationResult =
                await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            return await GetResourceAsyncInternal<T>(ParsedResourcePath, authorizationResult, userIdentity, options);
        }

        /// <inheritdoc/>
        public async Task<T> GetResourceAsync<T>(string instanceId, string resourceName, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
            where T : ResourceBase
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) =
                CreateAndValidateResourcePath(instanceId, HttpMethod.Get, typeof(T), resourceName);

            // Authorize access to the resource path.
            var authorizationResult =
                await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            return await GetResourceAsyncInternal<T>(ParsedResourcePath, authorizationResult, userIdentity, options);
        }

        /// <inheritdoc/>
        public async Task<TResult> UpsertResourceAsync<T, TResult>(string instanceId, T resource, UnifiedUserIdentity userIdentity, ResourceProviderUpsertOptions? options = null)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = CreateAndValidateResourcePath(instanceId, HttpMethod.Post, typeof(T), resourceName: resource.Name);

            // Authorize access to the resource path.
            var authorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            var upsertResult = await UpsertResourceAsyncInternal<T, TResult>(ParsedResourcePath, authorizationResult, resource, userIdentity, options);

            await UpsertResourcePostProcess(ParsedResourcePath, upsertResult, authorizationResult, userIdentity);

            return upsertResult;
        }

        /// <inheritdoc/>
        public async Task<TResult> UpdateResourcePropertiesAsync<T, TResult>(string instanceId, string resourceName, Dictionary<string, object?> propertyValues, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = CreateAndValidateResourcePath(instanceId, HttpMethod.Post, typeof(T), resourceName: resourceName);

            // Authorize access to the resource path.
            var authorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            var updateResult =
                await UpdateResourcePropertiesAsyncInternal<T, TResult>(ParsedResourcePath, authorizationResult, propertyValues, userIdentity);

            return updateResult;
        }

        /// <inheritdoc/>
        public async Task<TResult> ExecuteResourceActionAsync<T, TAction, TResult>(string instanceId, string resourceName, string actionName, TAction actionPayload, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TAction : class?
            where TResult : ResourceProviderActionResult
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) = CreateAndValidateResourcePath(instanceId, HttpMethod.Post, typeof(T), resourceName: resourceName, actionName: actionName);

            var authorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation,
                actionName == ResourceProviderActions.Filter, false, false);

            var actionResult =
                await ExecuteResourceActionAsyncInternal<T, TAction, TResult>(ParsedResourcePath, authorizationResult, actionPayload, userIdentity);

            return actionResult;
        }

        /// <inheritdoc/>
        public async Task<(bool Exists, bool Deleted)> ResourceExistsAsync<T>(string instanceId, string resourceName, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) =
                CreateAndValidateResourcePath(instanceId, HttpMethod.Get, typeof(T), resourceName: resourceName);

            // Authorize access to the resource path.
            var authorizationResult =
                await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            return await ResourceExistsAsyncInternal<T>(ParsedResourcePath, authorizationResult, userIdentity);
        }

        /// <inheritdoc/>
        public async Task DeleteResourceAsync<T>(string instanceId, string resourceName, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            EnsureServiceInitialization();
            var (ParsedResourcePath, AuthorizableOperation) =
                CreateAndValidateResourcePath(instanceId, HttpMethod.Get, typeof(T), resourceName: resourceName);

            // Authorize access to the resource path.
            var authorizationResult = await Authorize(ParsedResourcePath, userIdentity, AuthorizableOperation, false, false, false);

            await DeleteResourceAsyncInternal<T>(ParsedResourcePath, authorizationResult, userIdentity);
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <returns></returns>
        protected virtual async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
            where T : ResourceBase
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <typeparam name="T">The type of the resource being created or updated.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <param name="options">The <see cref="ResourceProviderUpsertOptions"/> which provides operation parameters.</param>
        /// <returns></returns>
        protected virtual async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpdateResourcePropertiesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <typeparam name="T">The type of the resource being updated.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="propertyValues">The dictionary with propery names and values to update.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        protected virtual async Task<TResult> UpdateResourcePropertiesAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            Dictionary<string, object> propertyValues,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult<T>
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of ResourceExistsAsync. Must be overridden in derived classes.
        /// </summary>
        /// <typeparam name="T">The type of resource being checked.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>A tuple indicating whether the resource exists or not and whether it is logically deleted or not.</returns>
        /// <remarks>
        /// If a resource was logically deleted but not purged, this method will return True, indicating the existence of the resource.
        /// </remarks>
        protected virtual async Task<(bool Exists, bool Deleted)> ResourceExistsAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            var resourceNameCheckResult = await CheckResourceName<T>(new ResourceName
            {
                Name = resourcePath.MainResourceId!
            });

            return
                (
                    resourceNameCheckResult.Exists,
                    resourceNameCheckResult.Deleted
                );
        }

        /// <summary>
        /// The internal implementation of ExecuteResourceActionAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="actionPayload">The <typeparamref name="TAction"/> object containing details about the action to be executed.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns>A <typeparamref name="TResult"/> object with the result of the action.</returns>
        /// <remarks>
        /// In the special case of the <c>filter</c> action, the override must handle the authorization result and return
        /// the appropriate response as follows:
        /// <list type="number">
        /// <item>The read action is authorized for the resource path itself.
        /// In this case, all matching resources must be returned according to the PBAC policies specified by the authorization result (if any).</item>
        /// <item>The read action is denied for the resource path itself.
        /// In this case, only the matching resources specified in the subordinate authorized resource paths list
        /// of the authorization result should be returned (if any).</item>
        /// </list>
        /// </remarks>
        protected virtual async Task<TResult> ExecuteResourceActionAsyncInternal<T, TAction, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            TAction actionPayload,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TAction : class?
            where TResult : ResourceProviderActionResult
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <typeparam name="T">The type of the resource being deleted.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task DeleteResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
            where T : ResourceBase
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Authorization

        /// <summary>
        /// Authorizes the specified action on a resource path.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> containing information about the identity of the user.</param>
        /// <param name="actionType">The type of action to be authorized (e.g., "read", "write", "delete").</param>
        /// <param name="expandResourceTypePaths">Indicates whether to expand resource type paths that are not authorized.</param>
        /// <param name="includeRoles">Indicates whether to include roles in the response.</param>
        /// <param name="includeActions">Indicates whether to include authorizable actions in the response.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        private async Task<ResourcePathAuthorizationResult> Authorize(ResourcePath resourcePath, UnifiedUserIdentity? userIdentity, string actionType,
            bool expandResourceTypePaths, bool includeRoles, bool includeActions)
        {
            try
            {
                if (userIdentity == null
                    || userIdentity.UserId == null)
                    throw new Exception("The provided user identity information cannot be used for authorization.");

                var rp = resourcePath.GetObjectId(_instanceSettings.Id, _name);
                var result = await _authorizationServiceClient.ProcessAuthorizationRequest(
                    _instanceSettings.Id,
                    $"{_name}/{resourcePath.MainResourceTypeName!}/{actionType}",
                    [rp],
                    expandResourceTypePaths,
                    includeRoles,
                    includeActions,
                    userIdentity);

                if (!result.AuthorizationResults[rp].Authorized
                    && !resourcePath.IsResourceTypePath
                    && result.AuthorizationResults[rp].PolicyDefinitionIds.Count == 0)
                {
                    // Only throw an exception if the resource path refers to a specific resource and there are no policies to enforce.
                    // For a resource path that refers to a specific resource, it is not acceptable to not be authorized directly
                    // if there are policies to enforce. Authorization might still fail later on (as a result of the policy enforcement),
                    // but at this point we don't need to throw.
                    // For a resource path that refers to a resource type, it is acceptable to not be authorized directly.
                    // When this happens, one of the following will occur:
                    // 1. The expandResourceTypePaths parameter is set to true, in which case the response will include
                    // any authorized subordinate resource paths (if there are none, the response will be empty).
                    // 2. The expandResourceTypePaths parameter is set to false, in which case the response will be empty.
                    throw new AuthorizationException("Access is not authorized.");
                }

                return result.AuthorizationResults[rp];
            }
            catch (AuthorizationException)
            {
                _logger.LogWarning("The {ActionType} access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                    actionType, resourcePath.GetObjectId(_instanceSettings.Id, _name), userIdentity!.Username, userIdentity!.UserId);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to authorize access to the resource path.");
                throw new ResourceProviderException(
                    "An error occurred while attempting to authorize access to the resource path.",
                    StatusCodes.Status403Forbidden);
            }
        }

        #endregion

        #region Events handling

        /// <summary>
        /// Sends a resource provider event to the event service.
        /// </summary>
        /// <param name="eventType">The type of the event to send.</param>
        /// <param name="data">The optional data to send with the event.</param>
        /// <returns></returns>
        /// <remarks>
        /// See <see cref="EventTypes"/> for a list of event types.
        /// </remarks>
        protected async Task SendResourceProviderEvent(string eventType, object? data = null)
        {
            if (_eventService == null)
            {
                _logger.LogWarning("The resource provider {ResourceProviderName} does not have an event service configured and cannot send events.", _name);
                return;
            }
                
            // The CloudEvent source is automatically filled in by the event service.
            await _eventService.SendEvent(
                EventGridTopics.FoundationaLLM_Resource_Providers,
                new CloudEvent(string.Empty, eventType, data ?? new { })
                {
                    Subject = _name
                });
        }
            

        private async Task HandleEvents(EventTypeEventArgs e)
        {
            // If the resource provider doesn't have any events to process, return.
            if(e.Events.Count == 0)
                return;

            var originalEventCount = e.Events.Count;

            // Only process events that are targeted for this resource provider.
            var eventsToProcess = e.Events
                .Where(e => e.Subject == _name).ToList();

            _logger.LogInformation("{EventsCount} events of type {EventType} received out if which {ResourceProviderEventsCount} are targeted for the {ResourceProviderName} resource provider.",
                originalEventCount,
                e.EventType,                
                eventsToProcess.Count,               
                _name);

            // If the resource provider doesn't have any events to process, return.
            if(eventsToProcess.Count == 0)
                return;

            // Handle the common events here and defer the rest to the derived classes.
            switch (e.EventType)
            {
                case EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand:
                    // No need to handle each event separately.
                    // If more than one event is received, this indicates that multiple cache resets were requested.
                    // However, the cache will be reset only once.
                    await HandleCacheResetCommand();
                    break;
                default:
                    await HandleEventsInternal(e);
                    break;
            }
        }

        /// <summary>
        /// Handles the cache reset command.
        /// </summary>
        /// <returns></returns>
        public virtual async Task HandleCacheResetCommand()
        {
            _resourceCache?.Reset();
            await (_resourceReferenceStore?.LoadResourceReferences() ?? Task.CompletedTask);
        }

        /// <summary>
        /// Handles events received from the <see cref="IEventService"/> when they are dequeued locally.
        /// </summary>
        /// <param name="e">The <see cref="EventTypeEventArgs"/> containing the event type and the actual events.</param>
        /// <returns></returns>
        protected virtual async Task HandleEventsInternal(EventTypeEventArgs e) =>
            await Task.CompletedTask;

        #endregion

        #region Internal validation

        private void EnsureServiceInitialization()
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
        }

        private (ResourcePath ParsedResourcePath, string AuthorizableOperation) CreateAndValidateResourcePath(
            string instanceId,
            HttpMethod operationType,
            Type resourceType,
            string? resourceName = null,
            string? actionName = null)
        {
            var hasAction = !string.IsNullOrWhiteSpace(actionName);
            var result = GetResourcePath(instanceId, resourceType, resourceName, actionName);
            var parsedResourcePath = new ResourcePath(
                result.ResourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: hasAction);

            if (hasAction)
            {
                var allowedTypes = result.ResourceTypeDescriptor.Actions?
                    .SingleOrDefault(a => a.Name == actionName)?
                    .AllowedTypes?
                    .SingleOrDefault(at => at.HttpMethod == operationType.Method)
                    ?? throw new ResourceProviderException(
                        $"The resource path {result.ResourcePath} does not support operation {operationType.Method}.",
                        StatusCodes.Status400BadRequest);
                return
                    (
                        parsedResourcePath,
                        allowedTypes.AuthorizableOperation
                    );
            }

            var resourceAllowedTypes =
                result.ResourceTypeDescriptor.AllowedTypes.SingleOrDefault(at => at.HttpMethod == operationType.Method)
                ?? throw new ResourceProviderException(
                    $"The HTTP method {operationType.Method} is not supported for resources of type {resourceType.Name} by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);

            return
                (
                    parsedResourcePath,
                    resourceAllowedTypes.AuthorizableOperation
                );
        }

        private (ResourcePath ParsedResourcePath, string AuthorizableOperation) ParseAndValidateResourcePath(
            string resourcePath,
            HttpMethod operationType,
            bool allowAction = true,
            Type? resourceType = null,
            bool requireResource = true)
        {
            var parsedResourcePath = new ResourcePath(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: allowAction);

            if (parsedResourcePath.ResourceTypeInstances.Count == 0)
                throw new ResourceProviderException(
                    $"The resource path {resourcePath} does not have any resource type instances and cannot be handled by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);

            ResourceTypeDescriptor? currentResourceTypeDescriptor = null;
            var currentAllowedResourceTypes = _allowedResourceTypes;

            foreach (var resourceTypeInstance in parsedResourcePath.ResourceTypeInstances)
            {
                if (currentAllowedResourceTypes == null
                    || !currentAllowedResourceTypes.TryGetValue(resourceTypeInstance.ResourceTypeName, out currentResourceTypeDescriptor))
                    throw new ResourceProviderException(
                        $"The resource type {resourceTypeInstance.ResourceTypeName} cannot be handled by the {_name} resource provider",
                        StatusCodes.Status400BadRequest);
                currentAllowedResourceTypes = currentResourceTypeDescriptor.SubTypes;
            }

            if (requireResource
                && !parsedResourcePath.HasResourceId)
                throw new ResourceProviderException(
                    $"The resource path {resourcePath} is required to have a resource identifier but none was found.",
                    StatusCodes.Status400BadRequest);

            if (!allowAction
                && parsedResourcePath.HasAction)
                throw new ResourceProviderException(
                    $"The resource path {resourcePath} is not allowed to have an action.",
                    StatusCodes.Status400BadRequest);

            if (resourceType != null
                && currentResourceTypeDescriptor!.ResourceType != resourceType)
                throw new ResourceProviderException(
                    $"The resource type {resourceType.Name} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);

            if (parsedResourcePath.HasAction)
            {
                var allowedTypes = currentResourceTypeDescriptor!.Actions?
                    .SingleOrDefault(a => a.Name == parsedResourcePath.Action)?
                    .AllowedTypes?
                    .SingleOrDefault(at => at.HttpMethod == operationType.Method)
                    ?? throw new ResourceProviderException(
                        $"The resource path {resourcePath} does not support operation {operationType.Method}.",
                        StatusCodes.Status400BadRequest);
                return (parsedResourcePath, allowedTypes.AuthorizableOperation);
            }
            else
            {
                var allowedTypes = currentResourceTypeDescriptor!.AllowedTypes?
                    .SingleOrDefault(at => at.HttpMethod == operationType.Method)
                    ?? throw new ResourceProviderException(
                        $"The resource path {resourcePath} does not support operation {operationType.Method}.",
                        StatusCodes.Status400BadRequest);
                return (parsedResourcePath, allowedTypes.AuthorizableOperation);
            }
        }

        #endregion

        #region Resource management

        /// <summary>
        /// Loads one or more resources of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of resources to load.</typeparam>
        /// <param name="instance">The <see cref="ResourceTypeInstance"/> that indicates a specific resource to load.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <param name="customResourceLoader">An optional function that loads the resource used to override
        /// the default resource loading mechanism.</param>
        /// <returns>A list of <see cref="ResourceProviderGetResult{T}"/> objects.</returns>
        protected async Task<List<ResourceProviderGetResult<T>>> LoadResources<T>(
            ResourceTypeInstance instance,
            ResourcePathAuthorizationResult authorizationResult,
            ResourceProviderGetOptions? options = null,
            Func<TResourceReference, bool, Task<T>>? customResourceLoader = null) where T : ResourceBase
        {
            Func<TResourceReference, Task<T>> resourceLoader =
                customResourceLoader == null
                    ? async (resourceReference) =>
                        (await LoadResource<T>(resourceReference))!
                    : async (resourceReference) =>
                        (await customResourceLoader(resourceReference, options?.LoadContent ?? false))!;

            IEnumerable<TResourceReference> resourceReferencesToLoad = [];

            // Keep the lock for the shortest possible time (until compiling the list of resource references to load).

            try
            {
                await _lock.WaitAsync();

                if (instance.ResourceId != null)
                {
                    // Loading a specific resource.
                    // No need to check the authorization result here, as it has already been checked by the Authorize method.
                    // The Authorize method is the one that produces the authorization result and it throws an exception if
                    // the authorization fails when loading a specific resource.
                    // See the comments inside the Authorize method for more details.

                    var resourceReference = await _resourceReferenceStore!.GetResourceReference(instance.ResourceId)
                        ?? throw new ResourceProviderException(
                            $"The resource reference for resource {instance.ResourceId} could not be found.",
                            StatusCodes.Status404NotFound);

                    resourceReferencesToLoad = [resourceReference];
                }
                else
                {
                    resourceReferencesToLoad = authorizationResult.Authorized
                        ? await _resourceReferenceStore!.GetAllResourceReferences<T>()
                        : await _resourceReferenceStore!.GetResourceReferences(
                            authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                                .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName))
                                .Select(sarp => sarp.ResourceName!)
                                .ToList());
                }

            }
            finally
            {
                _lock.Release();
            }

            // Proceed to load the resources

            if (instance.ResourceId != null)
            {
                // Handle the case of loading a specific resource separately.
                // This is because we need to throw in case the resource cannot be loaded.

                var resource = await resourceLoader(resourceReferencesToLoad.First());
                if (resource != null)
                {
                    return
                        [
                            new ResourceProviderGetResult<T>
                                {
                                    Resource = resource,
                                    Roles = (options?.IncludeRoles ?? false)
                                        ? authorizationResult.Roles
                                        : [],
                                    Actions = (options?.IncludeActions ?? false)
                                        ? authorizationResult.Actions
                                        : []
                                }
                        ];
                }
                else
                    throw new ResourceProviderException($"The resource {instance.ResourceId} could not be loaded.",
                        StatusCodes.Status500InternalServerError);
            }

            // Loading multiple resources of a specific type according to the authorization result.

            return await LoadResourcesFromReferences<T>(resourceReferencesToLoad, authorizationResult, resourceLoader, options);
        }

        /// <summary>
        /// Loads a resource based on its resource reference.
        /// </summary>
        /// <typeparam name="T">The type of resource to load.</typeparam>
        /// <param name="resourceReference">The type of resource reference used to indetify the resource to load.</param>
        /// <returns>The loaded resource.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        /// <remarks>
        /// Always ensure this method is called within a lock to avoid unexpected racing conditions.
        /// </remarks>
        protected async Task<T?> LoadResource<T>(TResourceReference resourceReference) where T : ResourceBase
        {
            if (!typeof(T).IsAssignableFrom(resourceReference.ResourceType))
                throw new ResourceProviderException(
                    $"The resource reference {resourceReference.Name} is not of the expected type {typeof(T).Name}.",
                    StatusCodes.Status400BadRequest);


            if (_resourceCache != null
                && _resourceCache.TryGetValue<T>(resourceReference, out T? cachedResource)
                && cachedResource != null)
                return cachedResource;

            if (await _storageService.FileExistsAsync(_storageContainerName, resourceReference.Filename, default))
            {
                var fileContent =
                    await _storageService.ReadFileAsync(_storageContainerName, resourceReference.Filename, default);

                try
                {
                    var resourceObject = JsonSerializer.Deserialize<T>(
                        Encoding.UTF8.GetString(fileContent.ToArray()),
                        _serializerSettings)
                            ?? throw new ResourceProviderException($"Failed to load the resource {resourceReference.Name}. Its content file might be corrupt.",
                                StatusCodes.Status500InternalServerError);

                    _resourceCache?.SetValue<T>(resourceReference, resourceObject);

                    return resourceObject;
                }
                catch (Exception ex)
                {
                    throw new ResourceProviderException($"Failed to load the resource {resourceReference.Name}. {ex.Message}.",
                                StatusCodes.Status500InternalServerError);
                }
            }

            return null;
        }

        /// <summary>
        /// Loads a resource based on its name.
        /// </summary>
        /// <typeparam name="T">The type of resource to load.</typeparam>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The loaded resource.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected async Task<T?> LoadResource<T>(string resourceName) where T : ResourceBase
        {
            try
            {
                await _lock.WaitAsync();

                var resourceReference = await _resourceReferenceStore!.GetResourceReference(resourceName)
                    ?? throw new ResourceProviderException($"Could not locate the {resourceName} resource.",
                        StatusCodes.Status404NotFound);

                if (_resourceCache != null
                    && _resourceCache.TryGetValue<T>(resourceReference, out T? cachedResource)
                    && cachedResource != null)
                    return cachedResource;

                if (await _storageService.FileExistsAsync(_storageContainerName, resourceReference.Filename, default))
                {
                    var fileContent =
                        await _storageService.ReadFileAsync(_storageContainerName, resourceReference.Filename, default);
                    var resourceObject = JsonSerializer.Deserialize<T>(
                        Encoding.UTF8.GetString(fileContent.ToArray()),
                        _serializerSettings)
                            ?? throw new ResourceProviderException($"Failed to load the resource {resourceReference.Name}. Its content file might be corrupt.",
                                StatusCodes.Status400BadRequest);

                    _resourceCache?.SetValue<T>(resourceReference, resourceObject);

                    return resourceObject;
                }

                return null;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Creates a resource based on a resource reference and the resource itself.
        /// </summary>
        /// <typeparam name="T">The type of resource to create.</typeparam>
        /// <param name="resourceReference">The resource reference used to identify the resource.</param>
        /// <param name="resource">The resource itself.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected async Task CreateResource<T>(TResourceReference resourceReference, T resource) where T : ResourceBase
        {
            try
            {
                await _lock.WaitAsync();

                if (resourceReference.ResourceType != resource.GetType())
                    throw new ResourceProviderException(
                        $"The resource reference {resourceReference.Name} is not of the expected type {typeof(T).Name}.",
                        StatusCodes.Status400BadRequest);

                await _storageService.WriteFileAsync(
                   _storageContainerName,
                   resourceReference.Filename,
                   JsonSerializer.Serialize<T>(resource, _serializerSettings),
                   default,
                default);

                await _resourceReferenceStore!.AddResourceReference(resourceReference);

                // Add resource to cache if caching is enabled.
                _resourceCache?.SetValue<T>(resourceReference, resource);

                await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Creates a resource based on a resource reference and the resource itself.
        /// </summary>
        /// <typeparam name="T">The type of resource to create.</typeparam>
        /// <param name="resourceReference">The resource reference used to identify the resource.</param>
        /// <param name="content">The resource itself.</param>
        /// <param name="contentType">The resource content type, if applicable.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected async Task CreateResource(TResourceReference resourceReference, Stream content, string? contentType)
        {
            try
            {
                await _lock.WaitAsync();

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    resourceReference.Filename,
                    content,
                    contentType ?? default,
                    default);

                if (_useInternalReferencesStore)
                    await _resourceReferenceStore!.AddResourceReference(resourceReference);

                await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Creates two resources based on their resource references and the resources themselves.
        /// </summary>
        /// <typeparam name="T1">The type of the first resource to create.</typeparam>
        /// <typeparam name="T2">The type of the second resource to create.</typeparam>
        /// <param name="resourceReference1">The resource reference used to identify the first resource.</param>
        /// <param name="resource1">The first resource to create.</param>
        /// <param name="resourceReference2">The resource reference used to identify the second resource.</param>
        /// <param name="resource2">The second resource to create.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected async Task CreateResources<T1, T2>(
            TResourceReference resourceReference1, T1 resource1,
            TResourceReference resourceReference2, T2 resource2)
            where T1 : ResourceBase
            where T2 : ResourceBase
        {
            try
            {
                await _lock.WaitAsync();

                if (resourceReference1.ResourceType != resource1.GetType())
                    throw new ResourceProviderException(
                        $"The resource reference {resourceReference1.Name} is not of the expected type {typeof(T1).Name}.",
                        StatusCodes.Status400BadRequest);

                if (resourceReference2.ResourceType != resource2.GetType())
                    throw new ResourceProviderException(
                        $"The resource reference {resourceReference2.Name} is not of the expected type {typeof(T2).Name}.",
                        StatusCodes.Status400BadRequest);

                await _storageService.WriteFileAsync(
                   _storageContainerName,
                   resourceReference1.Filename,
                   JsonSerializer.Serialize<T1>(resource1, _serializerSettings),
                   default,
                   default);

                await _storageService.WriteFileAsync(
                   _storageContainerName,
                   resourceReference2.Filename,
                   JsonSerializer.Serialize<T2>(resource2, _serializerSettings),
                   default,
                default);

                await _resourceReferenceStore!.AddResourceReferences(
                    [
                        resourceReference1,
                        resourceReference2
                    ]);

                await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Saves a resource based on its resource reference and the resource itself.
        /// </summary>
        /// <typeparam name="T">The type of resource to save.</typeparam>
        /// <param name="resourceReference">The resource reference used to identify the resource.</param>
        /// <param name="resource">The resource to be saved.</param>
        /// <returns></returns>
        protected async Task SaveResource<T>(TResourceReference resourceReference, T resource) where T : ResourceBase
        {
            try
            {
                await _lock.WaitAsync();

                await _storageService.WriteFileAsync(
                   _storageContainerName,
                   resourceReference.Filename,
                   JsonSerializer.Serialize<T>(resource, _serializerSettings),
                   default,
                   default);

                // Update resource to cache if caching is enabled.
                _resourceCache?.SetValue<T>(resourceReference, resource);

                await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Deletes a resource.
        /// </summary>
        /// <typeparam name="T">The type of resource to delete.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> identifying the resource to delete.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        /// <remarks>
        /// The operation is a logical delete. The resource reference is marked deleted, but the resource content remains in storage.
        /// To fully remove a resource, the delete operation must be followed by a purge operation.
        /// </remarks>
        protected async Task DeleteResource<T>(ResourcePath resourcePath)
        {
            var resourceName = resourcePath.ResourceId
                ?? throw new ResourceProviderException("The specified path does not contain a resource identifier.",
                    StatusCodes.Status400BadRequest);

            try
            {
                await _lock.WaitAsync();

                var result = await _resourceReferenceStore!.TryGetResourceReference(resourceName);

                if (result.Success
                    && !result.Deleted)
                {
                    await _resourceReferenceStore!.DeleteResourceReference(result.ResourceReference!);

                    await SendResourceProviderEvent(
                        EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
                }
                else
                {
                    throw new ResourceProviderException($"The resource {resourceName} cannot be deleted because it was either already deleted or does not exist.",
                        StatusCodes.Status404NotFound);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Purges a deleted resource.
        /// </summary>
        /// <typeparam name="T">The type of the resource to purge.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> identifying the resource to purge.</param>
        /// <returns>A <see cref="ResourceProviderActionResult"/> indicating the outcome of the operation.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        /// <remarks>
        /// The operation can only be applied to a resource that has been logically deleted.
        /// </remarks>
        protected async Task<ResourceProviderActionResult> PurgeResource<T>(ResourcePath resourcePath)
        {
            var resourceName = resourcePath.ResourceId
                ?? throw new ResourceProviderException("The specified path does not contain a resource identifier.",
                    StatusCodes.Status400BadRequest);

            try
            {
                await _lock.WaitAsync();

                var result = await _resourceReferenceStore!.TryGetResourceReference(resourceName);
                if (result.Success && result.Deleted)
                {
                    // Conditions are met to purge the resource.

                    // Delete the resource file from storage.
                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        result.ResourceReference!.Filename,
                        default);

                    // Remove the resource reference from the store.
                    await _resourceReferenceStore!.PurgeResourceReference(result.ResourceReference!);

                    await SendResourceProviderEvent(
                        EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);

                    return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
                }
                else
                {
                    throw new ResourceProviderException(
                        $"The resource {resourceName} cannot be purged because it is either not soft-deleted or does not exist.",
                        StatusCodes.Status400BadRequest);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Sets a resource as the default for its resource type.
        /// </summary>
        /// <typeparam name="T">The resource type.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> identifying the resource to which the default resource name should be set.</param>
        /// <returns>A <see cref="ResourceProviderActionResult"/> indicating the outcome of the operation.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected async Task<ResourceProviderActionResult> SetDefaultResource<T>(ResourcePath resourcePath)
        {
            var resourceName = resourcePath.ResourceId
                               ?? throw new ResourceProviderException("The specified path does not contain a resource identifier.",
                                   StatusCodes.Status400BadRequest);

            try
            {
                await _lock.WaitAsync();

                var result = await _resourceReferenceStore!.TryGetResourceReference(resourceName);
                if (result.Success && !result.Deleted)
                {
                    // Conditions are met to set the resource as the default.

                    // Set the default reference name in the store.
                    await _resourceReferenceStore!.SetDefaultResourceName(result.ResourceReference!);

                    await SendResourceProviderEvent(
                        EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);

                    return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
                }
                else
                {
                    throw new ResourceProviderException(
                        $"The resource {resourceName} cannot be set as the default because it is either deleted or does not exist.",
                        StatusCodes.Status400BadRequest);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Checks if a resource name is available.
        /// </summary>
        /// <typeparam name="T">The type of resource for which the name check is performed.</typeparam>
        /// <param name="resourceName">The <see cref="ResourceName"/> providing the name to be checked for availability.</param>
        /// <returns>A <see cref="ResourceNameCheckResult"/> indicating the outcome of the operation.</returns>
        protected async Task<ResourceNameCheckResult> CheckResourceName<T>(ResourceName resourceName)
        {
            var result = await _resourceReferenceStore!.TryGetResourceReference(resourceName.Name);

            // The name is denied if one of the following conditions is met:
            // 1. A resource with the specified name already exists (hence the reference was successfully retrieved).
            // 2. A resource with the specified name was previously deleted and not purged.
            return result.Success
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Exists = result.Success,
                    Deleted = result.Deleted,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed,
                    Exists = result.Success,
                    Deleted = result.Deleted
                };
        }

        /// <summary>
        /// Loads a list of resources filtered based on object IDs.
        /// </summary>
        /// <typeparam name="T">The type of resources to load.</typeparam>
        /// <param name="resourcePath">The <see cref="ResourcePath"/> resource type path to filter.</param>
        /// <param name="filter">The <see cref="ResourceFilter"/> used to filter the resources.</param>
        /// <param name="authorizationResult">The <see cref="ResourcePathAuthorizationResult"/> containing the result of the resource path authorization request.</param>
        /// <param name="options">The <see cref="ResourceProviderGetOptions"/> which provides operation parameters.</param>
        /// <param name="customResourceLoader">An optional function that loads the resource used to override
        /// the default resource loading mechanism.</param>
        /// <returns>A list of objects of type <typeparamref name="T"/>.</returns>
        protected async Task<IEnumerable<T>> FilterResources<T>(
            ResourcePath resourcePath,
            ResourceFilter filter,
            ResourcePathAuthorizationResult authorizationResult,
            ResourceProviderGetOptions? options = null,
            Func<TResourceReference, bool, Task<T>>? customResourceLoader = null)
            where T : ResourceBase
        {
            if (!resourcePath.IsResourceTypePath)
                throw new ResourceProviderException($"The resource path {resourcePath.RawResourcePath} is not a resource type path.",
                    StatusCodes.Status400BadRequest);

            Func<TResourceReference, Task<T>> resourceLoader =
                customResourceLoader == null
                    ? async (resourceReference) =>
                        (await LoadResource<T>(resourceReference))!
                    : async (resourceReference) =>
                        (await customResourceLoader(resourceReference, options?.LoadContent ?? false))!;

            List<string> filterResourceNames = [];

            if (filter.DefaultResource.HasValue
                && filter.DefaultResource.Value)
            {
                // Load the default resource for the resource type path.

                if (string.IsNullOrEmpty(_resourceReferenceStore!.DefaultResourceName))
                    throw new ResourceProviderException(
                        "The default resource name is not set for the resource provider.",
                        StatusCodes.Status500InternalServerError);

                filterResourceNames = [_resourceReferenceStore.DefaultResourceName];
            }
            else
            {
                // Filter resources based on the object IDs provided in the filter.

                var filterResourcePaths = filter.ObjectIDs?
                    .Select(id => this.GetParsedResourcePath(id, false))
                    .ToList()
                    ?? [];

                if (filterResourcePaths.Count == 0
                    || filterResourcePaths.Any(rp => !rp.HasResourceId || !rp.MatchesResourceTypes(resourcePath)))
                    throw new ResourceProviderException(
                        "The list of filter object IDs is either empty or contains invalid values.",
                        StatusCodes.Status400BadRequest);

                filterResourceNames = filterResourcePaths
                    .Select(rp => rp.ResourceId!)
                    .ToList();
            }

            IEnumerable<TResourceReference> resourceReferencesToLoad = [];

            // Keep the lock for the shortest possible time (until compiling the list of resource references to load).

            try
            {
                await _lock.WaitAsync();

                resourceReferencesToLoad = authorizationResult.Authorized
                    ? await _resourceReferenceStore!.GetResourceReferences(filterResourceNames)
                    : await _resourceReferenceStore!.GetResourceReferences(
                        authorizationResult.SubordinateResourcePathsAuthorizationResults.Values
                            .Where(sarp => !string.IsNullOrWhiteSpace(sarp.ResourceName) && filterResourceNames.Contains(sarp.ResourceName))
                            .Select(sarp => sarp.ResourceName!)
                            .ToList());
            }
            finally
            {
                _lock.Release();
            }

            var result = await LoadResourcesFromReferences<T>(
                resourceReferencesToLoad,
                authorizationResult,
                async (resourceReference) =>
                        (await resourceLoader(resourceReference))!,
                options);

            return result.Select(r => r.Resource);
        }

        #region Helpers

        private async Task<List<ResourceProviderGetResult<T>>> LoadResourcesFromReferences<T>(
            IEnumerable<TResourceReference> resourceReferences,
            ResourcePathAuthorizationResult authorizationResult,
            Func<TResourceReference, Task<T>> resourceLoader,
            ResourceProviderGetOptions? options = null) where T : ResourceBase
        {
            List<ResourceProviderGetResult<T>> results = [];

            foreach (var resourceReference in resourceReferences)
            {
                // Attempt to identify the subordinate authorization result for the intermediate resource.
                authorizationResult.SubordinateResourcePathsAuthorizationResults.TryGetValue(
                    resourceReference.Name,
                    out ResourcePathAuthorizationResult? subordinateAuthorizationResult);

                // An intermediate resource will be returned only if one of the following conditions is met:
                // 1. The resource type path itself is authorized.
                // 2. The resource type path itself is not authorized, but the intermediate resource exists in the
                // subordinate resource paths authorization results and is authorized.
                if (authorizationResult.Authorized
                    || (subordinateAuthorizationResult?.Authorized ?? false))
                {
                    var loadedResource = await resourceLoader(resourceReference);
                    if (loadedResource != null)
                        results.Add(
                            new ResourceProviderGetResult<T>
                            {
                                Resource = loadedResource,
                                Roles = (options?.IncludeRoles ?? false)
                                    ? authorizationResult.Roles
                                        .Union(subordinateAuthorizationResult?.Roles ?? [])
                                        .ToList()
                                    : [],
                                Actions = (options?.IncludeActions ?? false)
                                    ? authorizationResult.Actions
                                        .Union(subordinateAuthorizationResult?.Actions ?? [])
                                        .ToList()
                                    : []
                            });
                }
            }

            // If a resource name matches the default resource name in the resource reference store, add a new property value to the resource.
            if (!string.IsNullOrWhiteSpace(_resourceReferenceStore?.DefaultResourceName) && results
                    .Any(a => a.Resource.Name == _resourceReferenceStore.DefaultResourceName))
            {
                var resource = results.First(a => a.Resource.Name == _resourceReferenceStore.DefaultResourceName)
                    .Resource;
                if (resource.Properties != null)
                {
                    resource.Properties[ResourcePropertyNames.DefaultResource] = true.ToString().ToLowerInvariant();
                }
                else
                {
                    resource.Properties = new Dictionary<string, string>
                    {
                        { ResourcePropertyNames.DefaultResource, true.ToString().ToLowerInvariant() }
                    };
                }

                foreach (var result in results.Where(r => r.Resource.Name != _resourceReferenceStore.DefaultResourceName))
                {
                    result.Resource.Properties?.Remove(ResourcePropertyNames.DefaultResource);
                }
            }

            return results;
        }

        #endregion

        #endregion

        #region Utils

        /// <summary>
        /// Gets a resource provider service by name.
        /// </summary>
        /// <param name="name">The name of the resource provider.</param>
        /// <returns>The <see cref="IResourceProviderService"/> used to interact with the resource provider.</returns>
        protected IResourceProviderService GetResourceProviderServiceByName(string name)
        {
            if (!_resourceProviders.ContainsKey(name))
                _resourceProviders.Add(
                    name,
                    _serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                        .Single(rp => rp.Name == name));
            return _resourceProviders[name];
        }

        /// <summary>
        /// Updates the base properties of an object derived from <see cref="ResourceBase"/>.
        /// </summary>
        /// <param name="resource">The <see cref="ResourceBase"/> object to be updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the information about the identity of the user that performed a create or update operation on the resource.</param>
        /// <param name="isNew">Indicates whether the resource is new or being updated.</param>
        protected void UpdateBaseProperties(ResourceBase resource, UnifiedUserIdentity userIdentity, bool isNew = false)
        {
            if (isNew)
            {
                // The resource was just created
                resource.CreatedBy = userIdentity.UPN ?? userIdentity.UserId ?? "N/A";
                resource.CreatedOn = DateTimeOffset.UtcNow;
            }
            else
            {
                // The resource was updated
                resource.UpdatedBy = userIdentity.UPN ?? userIdentity.UserId ?? "N/A";
                resource.UpdatedOn = DateTimeOffset.UtcNow;
            }
        }

        /// <summary>
        /// Get the fully qualified resource path for a specified resource.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <returns></returns>
        protected (string ResourcePath, ResourceTypeDescriptor ResourceTypeDescriptor) GetResourcePath(string instanceId, Type resourceType, string? resourceName = null, string? actionName = null)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ResourceProviderException(
                    $"The FoundationaLLM instance identifier is invalid.",
                    StatusCodes.Status400BadRequest);
                       
            var resourceTypeDescriptor =
                AllowedResourceTypes.Values.SingleOrDefault(art => art.ResourceType == resourceType)
                ?? throw new ResourceProviderException(
                    $"The resource type {resourceType.Name} is not supported by the {Name} resource provider.",
                    StatusCodes.Status400BadRequest);

            return
                (
                    string.IsNullOrWhiteSpace(resourceName)
                        ? (string.IsNullOrWhiteSpace(actionName)
                            ? $"/instances/{instanceId}/providers/{Name}/{resourceTypeDescriptor.ResourceTypeName}"
                            : $"/instances/{instanceId}/providers/{Name}/{resourceTypeDescriptor.ResourceTypeName}/{actionName}")
                        : (string.IsNullOrWhiteSpace(actionName)
                            ? $"/instances/{instanceId}/providers/{this.Name}/{resourceTypeDescriptor.ResourceTypeName}/{resourceName}"
                            : $"/instances/{instanceId}/providers/{this.Name}/{resourceTypeDescriptor.ResourceTypeName}/{resourceName}/{actionName}"),
                    resourceTypeDescriptor
                );
        }

        /// <summary>
        /// Gets a <see cref="ResourcePath"/> object for the specified string resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="allowAction">Indicates whether actions are allowed in the resource path.</param>
        /// <returns>A <see cref="ResourcePath"/> object.</returns>
        protected ResourcePath GetParsedResourcePath(string resourcePath, bool allowAction = true) =>
            new(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: allowAction);

        private async Task UpsertResourcePostProcess(
            ResourcePath resourcePath,
            ResourceProviderUpsertResult upsertResult,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            ArgumentNullException.ThrowIfNull(resourcePath, nameof(resourcePath));
            ArgumentNullException.ThrowIfNull(upsertResult, nameof(upsertResult));
            ArgumentNullException.ThrowIfNull(authorizationResult, nameof(authorizationResult));
            ArgumentException.ThrowIfNullOrWhiteSpace(userIdentity.UserId, nameof(userIdentity.UserId));
            ArgumentException.ThrowIfNullOrWhiteSpace(userIdentity.Name, nameof(userIdentity.Name));
            ArgumentException.ThrowIfNullOrWhiteSpace(resourcePath.InstanceId, nameof(resourcePath.InstanceId));

            if (!authorizationResult.Authorized)
                throw new ResourceProviderException(
                    $"Upsert result post-processing can only be executed on authorized resources.");

            // Owner role assignment is already set on previously created resources.
            if (upsertResult.ResourceExists)
                return;

            // Owner role assignment is not required on Authorization resources.
            if (Name == ResourceProviderNames.FoundationaLLM_Authorization)
                return;

            // Owner role assignment is not required when at least one policy is assigned to the resource path.
            if (authorizationResult.PolicyDefinitionIds.Count > 0)
                return;

            // Owner role assignment is not required on subordinate resources.
            if (resourcePath.HasSubordinateResourceId)
                return;

            var roleAssignmentName = Guid.NewGuid().ToString();
            var roleAssignmentDescription = $"Owner role for {userIdentity.Name}";
            var roleAssignmentResult = await _authorizationServiceClient.CreateRoleAssignment(
                _instanceSettings.Id,
                new RoleAssignmentRequest()
                {
                    Name = roleAssignmentName,
                    Description = roleAssignmentDescription,
                    ObjectId = $"/instances/{resourcePath.InstanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                    PrincipalId = userIdentity.UserId,
                    PrincipalType = PrincipalTypes.User,
                    RoleDefinitionId = $"/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleDefinitions}/{RoleDefinitionNames.Owner}",
                    Scope = upsertResult.ObjectId
                },
                userIdentity);

            if (!roleAssignmentResult.Success)
                _logger.LogError("The [{RoleAssignment}] could not be assigned to {ObjectId}.", roleAssignmentDescription, upsertResult.ObjectId);
        }

        public PolicyDefinition EnsureAndValidatePolicyDefinitions(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult)
        {
            // The FoundationaLLM.Conversation resource provider is opinionated about the specific PBAC policy assignment required to load resources.

            if (authorizationResult.PolicyDefinitionIds.Count == 0)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires PBAC policy assignments to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            if (authorizationResult.PolicyDefinitionIds.Count > 1)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires exactly one PBAC policy assignment to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            if (!PolicyDefinitions.All.TryGetValue(authorizationResult.PolicyDefinitionIds[0], out var policyDefinition))
                throw new ResourceProviderException(
                    $"The {_name} resource provider did not find the PBAC policy with id {authorizationResult.PolicyDefinitionIds[0]} required to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            var userIdentityProperties = policyDefinition.MatchingStrategy?.UserIdentityProperties ?? [];
            if (userIdentityProperties.Count != 1
                || userIdentityProperties[0] != UserIdentityPropertyNames.UserPrincipalName)
                throw new ResourceProviderException(
                    $"The {_name} resource provider requires one PBAC policy assignment with a matching strategy based on the user principal name (UPN) to load the {resourcePath.RawResourcePath} resource path.",
                    StatusCodes.Status500InternalServerError);

            return policyDefinition;
        }

        #endregion
    }
}
