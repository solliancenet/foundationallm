using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Maintains a collection of resource references used by FoundationaLLM resource providers.
    /// </summary>
    /// <typeparam name="T">The type of resource reference kept in the store.</typeparam>
    /// <param name="resourceProvider">The <see cref="IResourceProviderService"/> resource provider service that uses the reference store.</param>
    /// <param name="resourceProviderStorageService">The <see cref="IStorageService"/> used by the resource provider.</param>
    /// <param name="logger">The <see cref="ILogger"/> service used by the resource provider.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used by the resource provider to signal the need to cancel operations.</param>
    public class ResourceProviderResourceReferenceStore<T>(
        IResourceProviderService resourceProvider,
        IStorageService resourceProviderStorageService,
        ILogger logger,
        CancellationToken cancellationToken = default)
        where T : ResourceReference
    {
        private readonly IResourceProviderService _resourceProvider = resourceProvider;
        private readonly IStorageService _storage = resourceProviderStorageService;
        private readonly ILogger _logger = logger;
        private readonly CancellationToken _cancellationToken = cancellationToken;

        private readonly Dictionary<string, T> _resourceReferences = [];

        private const string RESOURCE_REFERENCES_FILE_NAME = "_resource-references.json";
        private string ResourceReferencesFilePath => $"/{_resourceProvider.Name}/{RESOURCE_REFERENCES_FILE_NAME}";

        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private string? _defaultResourceName;

        /// <summary>
        /// Gets the name of the default resource (if any).
        /// </summary>
        public string? DefaultResourceName => _defaultResourceName;

        /// <summary>
        /// Loads the resource references from the storage service.
        /// </summary>
        /// <returns></returns>
        public async Task LoadResourceReferences()
        {
            _logger.LogInformation($"Starting to load the references for the {_resourceProvider.Name} resource provider...");

            await _lock.WaitAsync();
            try
            {
                if (await _storage.FileExistsAsync(
                    _resourceProvider.StorageContainerName,
                    ResourceReferencesFilePath,
                    _cancellationToken))
                {
                   await LoadAndMergeResourceReferences();
                }
                else
                {
                    // The resource references file does not exist, so we need to create it.
                    await SaveResourceReferences();
                }

                _logger.LogInformation(
                    "The references for the {ResourceProviderName} were successfully loaded.",
                    _resourceProvider.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "There was an error while loading the resource references for the {ResourceProviderName} resource provider.",
                    _resourceProvider.Name);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns></returns>
        public async Task<T?> GetResourceReference(string resourceName)
        {
            await _lock.WaitAsync();
            try
            {
                var resourceReference = GetResourceReferenceInternal(resourceName);

                if (resourceReference == null)
                {
                    // The reference was not found which means it either does not exist or has been created by another instance of the resource provider.

                    // Wait for a second to ensure that potential reference creation processes happening in different instances of the resource provider have completed.
                    await Task.Delay(10, _cancellationToken);

                    await LoadAndMergeResourceReferences();
                }

                // Try getting the reference again.
                resourceReference = GetResourceReferenceInternal(resourceName);

                // Return the result, regardless of whether it is null or not.
                // If it is null, the caller will have to handle the situation.
                return resourceReference;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Attempts to get a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>
        /// A tuple containing a boolean value (Success) indicating whether the resource reference was successfully retrieved,
        /// a boolean value (Deleted) indicating whether the resource is deleted (and not purged), and the resource reference itself (ResourceReference).
        /// </returns>
        /// <remarks>
        /// <para>
        /// When Success is <c>false</c>, the ResourceReference will be <c>null</c>.
        /// This means that the resource reference was not found in the store.
        /// </para>
        /// <para>
        /// When Success is <c>true</c>, the ResourceReference will contain the reference to the resource.
        /// This means that the resource reference is either valid or it has been logically deleted.
        /// Callers should check the Deleted value to determine whether the resource was logically deleted.
        /// </para>
        /// </remarks>
        public async Task<(bool Success, bool Deleted, T? ResourceReference)> TryGetResourceReference(string resourceName)
        {
            await _lock.WaitAsync();
            try
            {
                var success = TryGetResourceReferenceInternal(resourceName, out var deleted, out var resourceReference);

                if (success)
                    return (true, deleted, resourceReference);

                // The reference was not found which means it either does not exist or has been created by another instance of the resource provider.

                // Wait for 100 miliseconds to ensure that potential reference creation processes happening in different instances of the resource provider have completed.
                await Task.Delay(100, _cancellationToken);

                await LoadAndMergeResourceReferences();


                // Try getting the reference again.
                success = TryGetResourceReferenceInternal(resourceName, out deleted, out resourceReference);

                // Return the result, regardless of whether it is null or not.
                // If it is null, the caller will have to handle the situation.
                return (success, deleted, resourceReference);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Filters the resource references in the store based on the predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter the resource references.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is not safe in scenarios where multiple instances of a resource provider are running at the same time.
        /// </remarks>
        public async Task<IEnumerable<T>> GetResourceReferences(Func<T, bool> predicate)
        {
            await _lock.WaitAsync();
            try
            {
                return _resourceReferences.Values.Where(rr => predicate(rr) && !rr.Deleted);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets the resource references for the specified resource names.
        /// </summary>
        /// <param name="resourceNames">The list of resource names for which the references should be retrieved.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetResourceReferences(IEnumerable<string> resourceNames)
        {
            await _lock.WaitAsync();
            try
            {
                if (resourceNames.Except(_resourceReferences.Keys).Any())
                {
                    // Some of the resource references are missing, so we need to load them.
                    await LoadAndMergeResourceReferences();
                }

                return resourceNames
                    .Select(rn => _resourceReferences.GetValueOrDefault(rn))
                    .Where(rr => (rr != null) && !rr.Deleted)!;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets all resource references of type T1 in the store.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> contain</returns>
        /// <remarks>
        /// This method is not safe in scenarios where multiple instances of a resource provider are running at the same time.
        /// </remarks>
        public async Task<IEnumerable<T>> GetAllResourceReferences<T1>() where T1 : ResourceBase
        {
            await _lock.WaitAsync();
            try
            {
                return [.. _resourceReferences.Values.Where(rr => !rr.Deleted && typeof(T1).IsAssignableFrom(rr.ResourceType))];
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Adds a resource reference to the store.
        /// </summary>
        /// <param name="resourceReference">The resource reference to add.</param>
        /// <returns></returns>
        public async Task UpsertResourceReference(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);             
                _resourceReferences[resourceReference.Name] = resourceReference;
                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Adds a resource reference to the store.
        /// </summary>
        /// <param name="resourceReference">The resource reference to add.</param>
        /// <returns></returns>
        public async Task AddResourceReference(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);

                if (existingResourceReference != null)
                    throw new ResourceProviderException(
                        $"A resource reference for the resource {resourceReference.Name} already exists.",
                        StatusCodes.Status400BadRequest);

                _resourceReferences[resourceReference.Name] = resourceReference;

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Adds a resource reference to the store.
        /// </summary>
        /// <param name="resourceReferences">The list of resource references to add.</param>
        /// <returns></returns>
        public async Task AddResourceReferences(IEnumerable<T> resourceReferences)
        {
            await _lock.WaitAsync();
            try
            {
                foreach (var resourceReference in resourceReferences)
                {
                    var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);

                    if (existingResourceReference != null)
                        throw new ResourceProviderException(
                            $"A resource reference for the resource {resourceReference.Name} already exists.",
                            StatusCodes.Status400BadRequest);

                    _resourceReferences[resourceReference.Name] = resourceReference;
                }

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Loads the persisted resource references and merges them with the existing references.
        /// </summary>
        /// <remarks>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </remarks>
        private async Task LoadAndMergeResourceReferences()
        {
            var fileContent = await _storage.ReadFileAsync(
                        _resourceProvider.StorageContainerName,
                        ResourceReferencesFilePath,
                        _cancellationToken);
            var persistedReferencesList = JsonSerializer.Deserialize<ResourceReferenceList<T>>(
                Encoding.UTF8.GetString(fileContent.ToArray()))!;

            _defaultResourceName = persistedReferencesList.DefaultResourceName;

            foreach (var reference in persistedReferencesList.ResourceReferences)
            {
                if (_resourceReferences.TryGetValue(reference.Name, out var existingReference))
                {
                    if (!existingReference.Equals(reference))
                    {
                        _resourceReferences[reference.Name] = reference;
                    }
                }
                else
                {
                    _resourceReferences[reference.Name] = reference;
                }
            }

            var referenceNamesToRemove = _resourceReferences.Keys
                .Except(persistedReferencesList.ResourceReferences.Select(rr => rr.Name))
                .ToList();
            foreach (var referenceNameToRemove in referenceNamesToRemove)
                _resourceReferences.Remove(referenceNameToRemove);
        }

        /// <summary>
        /// Sets the default resource name on the store.
        /// </summary>
        /// <param name="resourceReference">The resource reference to set as the default.</param>
        /// <returns></returns>
        public async Task SetDefaultResourceName(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                var existingResourceReference = GetResourceReferenceInternal(resourceReference.Name);

                if (existingResourceReference == null)
                    throw new ResourceProviderException(
                        $"Cannot set the default resource name to {resourceReference.Name} since it does not exist in the resource reference store.",
                        StatusCodes.Status400BadRequest);

                _defaultResourceName = resourceReference.Name;

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Deletes a resource reference from the store.
        /// </summary>
        /// <param name="resourceReference">The name of the resource to delete.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        public async Task DeleteResourceReference(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                resourceReference.Deleted = true;

                await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceReference"></param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        public async Task PurgeResourceReference(T resourceReference)
        {
            await _lock.WaitAsync();
            try
            {
                if (!resourceReference.Deleted)
                    throw new ResourceProviderException(
                        $"The resource reference for the resource {resourceReference.Name} cannot be purged. "
                        + "It is not marked as deleted.",
                        StatusCodes.Status400BadRequest);

                if (!_resourceReferences.Remove(resourceReference.Name))
                    _logger.LogWarning(
                        "The resource reference for the resource {ResourceName} could not be purged. "
                        + "It was not found in the resource references store.",
                        resourceReference.Name);
                else
                    await SaveResourceReferences();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Saves the resource references to the storage service.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </remarks>
        private async Task SaveResourceReferences() =>
            await _storage.WriteFileAsync(
                _resourceProvider.StorageContainerName,
                ResourceReferencesFilePath,
                JsonSerializer.Serialize(new ResourceReferenceList<T>
                {
                    DefaultResourceName = _defaultResourceName,
                    ResourceReferences = _resourceReferences.Values.ToList()
                }),
                default,
                _cancellationToken);

        /// <summary>
        /// Gets a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        /// <remarks>
        /// <para>If the resource exists and it was deleted without being also purged, an exception is thrown.</para>
        /// <para>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </para>
        /// </remarks>
        private T? GetResourceReferenceInternal(string resourceName)
        {
            if (_resourceReferences.TryGetValue(resourceName, out var reference))
            {
                if (reference == null
                    || reference.Deleted)
                    throw new ResourceProviderException(
                        $"The resource reference for the resource {resourceName} cannot be retrieved. "
                        + "It is either null or points to a resource that has been deleted but not purged yet.",
                        StatusCodes.Status400BadRequest);

                return reference;
            }
            else
                return null;
        }

        /// <summary>
        /// Attempts to get a resource reference by the unique name of the resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="deleted">Indicates whether the resource was deleted and not purged (in this case the value is <c>true</c>).</param>
        /// <param name="resourceReference">The resource reference that matches the resource name.</param>
        /// <returns><c>True</c> if the resource reference was successfully retrieved.</returns>
        /// <remarks>
        /// <para>If the resource exists and it was deleted without being also purged, the result will be <c>false</c> and output <c>null</c>.</para>
        /// <para>
        /// IMPORTANT!
        /// Never call this method without acquiring the lock first.
        /// </para>
        /// </remarks>
        private bool TryGetResourceReferenceInternal(string resourceName, out bool deleted, out T? resourceReference)
        {
            resourceReference = null;
            deleted = false;

            if (_resourceReferences.TryGetValue(resourceName, out var reference))
            {
                if (reference == null)
                    return false;

                if (reference.Deleted)
                    deleted = true;

                resourceReference = reference;
                return true;
            }
            else
                return false;
        }
    }
}
