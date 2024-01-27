using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProvider;
using Microsoft.Extensions.Logging;

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

        /// <inheritdoc/>
        public string Name => _name;

        /// <inheritdoc/>
        public bool IsInitialized  => _isInitialized;

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="logger">The logger used for logging.</param>
        public ResourceProviderServiceBase(
            IStorageService storageService,
            ILogger logger)
        {
            _storageService = storageService;
            _logger = logger;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        /// <inheritdoc/>
        public async Task Initialize()
        {
            await InitializeInternal();
            _isInitialized = true;
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> ExecuteAction(string actionPath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(actionPath);
            return await ExecuteActionInternal(instances);
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
        public async Task UpsertResourceAsync<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await UpsertResourceAsync<T>(resourcePath, resource);
        }

        /// <inheritdoc/>
        public void UpsertResource<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            UpsertResource<T>(instances, resource);
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
        public void DeleteResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            DeleteResource<T>(instances);
        }

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

        private List<ResourceTypeInstance> GetResourceInstancesFromPath(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.");

            var tokens = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
                throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.");

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

                if (currentResourceType.Actions.Contains(tokens[currentIndex + 1]))
                {
                    resourceTypeInstance.Action = tokens[currentIndex + 1];
                    break;
                }

                resourceTypeInstance.ResourceId = tokens[currentIndex + 1];

                currentResourceTypes = currentResourceType.SubTypes;
                currentIndex += 2;
            }

            return result;
        }
    }
}
