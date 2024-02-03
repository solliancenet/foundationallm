using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Implements basic resource provider functionality
    /// </summary>
    public class ResourceProviderServiceBase : IResourceProviderService
    {
        private bool _isInitialized = false;

        /// <summary>
        /// The <see cref="IStorageService"/> providing storage services to the resource provider.
        /// </summary>
        protected readonly IStorageService _storageService;

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
        /// The resource descriptors of the resource types managed by the resource provider. Must be overridden in derived classes.
        /// </summary>
        protected virtual Dictionary<string, ResourceTypeDescriptor> _resourceTypes => [];

        /// <summary>
        /// The name of the resource provider. Must be overridden in derived classes.
        /// </summary>
        protected virtual string _name => throw new NotImplementedException();

        /// <summary>
        /// Default JSON serialization settings.
        /// </summary>
        protected virtual JsonSerializerSettings _serializerSettings => new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        /// <inheritdoc/>
        public string Name => _name;

        /// <inheritdoc/>
        public bool IsInitialized  => _isInitialized;

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="instanceSettings">The <see cref="InstanceSettings"/> that provides instance-wide settings.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="logger">The logger used for logging.</param>
        public ResourceProviderServiceBase(
            InstanceSettings instanceSettings,
            IStorageService storageService,
            ILogger logger)
        {
            _storageService = storageService;
            _logger = logger;
            _instanceSettings = instanceSettings;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        /// <inheritdoc/>
        private async Task Initialize()
        {
            try
            {
                await InitializeInternal();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The resource provider {ResourceProviderName} failed to initialize.", _name);
            }
        }

        #region IResourceProviderService

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> ExecuteAction(string actionPath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(actionPath);
            return await ExecuteActionInternal(instances);
        }

        /// <inheritdoc/>
        public IList<T> GetResources<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return GetResourcesInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<IList<T>> GetResourcesAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return await GetResourcesAsyncInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<string> GetResourcesAsync(string resourcePath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return await GetResourcesAsyncInternal(instances);
        }

        /// <inheritdoc/>
        public T GetResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return GetResourceInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<T> GetResourceAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return await GetResourceAsyncInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await UpsertResourceAsync<T>(instances, resource);
            return GetObjectId(instances);
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync(string resourcePath, string serializedResource)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await UpsertResourceAsync(instances, serializedResource);
            return GetObjectId(instances);
        }

        /// <inheritdoc/>
        public string UpsertResource<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            UpsertResource<T>(instances, resource);
            return GetObjectId(instances);
        }    

        /// <inheritdoc/>
        public async Task DeleteResourceAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await DeleteResourceAsync<T>(instances);
        }

        /// <inheritdoc/>
        public async Task DeleteResourceAsync(string resourcePath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await DeleteResourceAsync(instances);
        }

        /// <inheritdoc/>
        public void DeleteResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            DeleteResource<T>(instances);
        }

        #endregion

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
        /// The internal implementation of ExecuteAction. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<ResourceProviderActionResult> ExecuteActionInternal(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of GetResources. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual IList<T> GetResourcesInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<IList<T>> GetResourcesAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<string> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of GetResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual T GetResourceInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of GetResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<T> GetResourceAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns></returns>
        protected virtual void UpsertResource<T>(List<ResourceTypeInstance> instances, T resource) =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task UpsertResourceAsync<T>(List<ResourceTypeInstance> instances, T resource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="serializedResource">The serialized resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual void DeleteResource<T>(List<ResourceTypeInstance> instances) =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync<T>(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the resource unique identifier based on the resource path.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns>The unique resource identifier.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected string GetObjectId(List<ResourceTypeInstance> instances)
        {
            foreach (var instance in instances)
                if (string.IsNullOrWhiteSpace(instance.ResourceType)
                    || string.IsNullOrWhiteSpace(instance.ResourceId)
                    || !(instance.Action == null))
                    throw new ResourceProviderException("The provided resource path is not a valid resource identifier.");

            return $"/instances/{_instanceSettings.Id}/providers/{_name}/{string.Join("/",
                instances.Select(i => $"{i.ResourceType}/{i.ResourceId}").ToArray())}";
        }

        private List<ResourceTypeInstance> GetResourceInstancesFromPath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.");

            var tokens = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var result = new List<ResourceTypeInstance>();
            var currentResourceTypes = _resourceTypes;
            var currentIndex = 0;
            while (currentIndex < tokens.Length)
            {
                if (currentResourceTypes == null
                    || !currentResourceTypes.TryGetValue(tokens[currentIndex], out ResourceTypeDescriptor? currentResourceType))
                    throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.");

                var resourceTypeInstance = new ResourceTypeInstance(tokens[currentIndex]);
                result.Add(resourceTypeInstance);

                if (currentIndex + 1 == tokens.Length)
                    // No more tokens left, which means we have a resource type instance without actions or subtypes.
                    // This will be used by resource providers to retrieve all resources of a specific resource type.
                    break;

                resourceTypeInstance.ResourceId = tokens[currentIndex + 1];

                if (currentIndex + 2 == tokens.Length - 1)
                {
                    // Only one token left after the resource identifier.
                    // This means it can only be an action.
                    if (currentResourceType.Actions.Contains(tokens[currentIndex + 2]))
                    {
                        // The token represents an action.
                        resourceTypeInstance.Action = tokens[currentIndex + 2];
                        break;
                    }
                    else
                        throw new ResourceProviderException($"The [{tokens[currentIndex + 2]}] action is invalid.");
                }

                currentResourceTypes = currentResourceType.SubTypes;
                currentIndex += 2;
            }

            return result;
        }
    }
}
