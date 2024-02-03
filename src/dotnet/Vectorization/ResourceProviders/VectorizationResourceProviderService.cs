using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class VectorizationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)] IStorageService storageService,
        ILogger<VectorizationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            storageService,
            logger)
    {
        private Dictionary<string, ContentSourceProfile> _contentSourceProfiles = [];
        private Dictionary<string, TextPartitioningProfile> _textPartitioningProfiles = [];
        private Dictionary<string, TextEmbeddingProfile> _textEmbeddingProfiles = [];
        private Dictionary<string, IndexingProfile> _indexingProfiles = [];

        private const string CONTENT_SOURCE_PROFILES_FILE_NAME = "vectorization-content-source-profiles.json";
        private const string TEXT_PARTITIONING_PROFILES_FILE_NAME = "vectorization-text-partitioning-profiles.json";
        private const string TEXT_EMBEDDING_PROFILES_FILE_NAME = "vectorization-text-embedding-profiles.json";
        private const string INDEXING_PROFILES_FILE_NAME = "vectorization-indexing-profiles.json";

        private const string CONTENT_SOURCE_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{CONTENT_SOURCE_PROFILES_FILE_NAME}";
        private const string TEXT_PARTITIONING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_PARTITIONING_PROFILES_FILE_NAME}";
        private const string TEXT_EMBEDDING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{TEXT_EMBEDDING_PROFILES_FILE_NAME}";
        private const string INDEXING_PROFILES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Vectorization}/{INDEXING_PROFILES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Vectorization;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> _resourceTypes => new()
        {
            {
                VectorizationResourceTypeNames.VectorizationRequests,
                new ResourceTypeDescriptor(VectorizationResourceTypeNames.VectorizationRequests)
            },
            {
                VectorizationResourceTypeNames.ContentSourceProfiles,
                new ResourceTypeDescriptor(VectorizationResourceTypeNames.ContentSourceProfiles)
            },
            {
                VectorizationResourceTypeNames.TextPartitioningProfiles,
                new ResourceTypeDescriptor(VectorizationResourceTypeNames.TextPartitioningProfiles)
            },
            {
                VectorizationResourceTypeNames.TextEmbeddingProfiles,
                new ResourceTypeDescriptor(VectorizationResourceTypeNames.TextEmbeddingProfiles)
            },
            {
                VectorizationResourceTypeNames.IndexingProfiles,
                new ResourceTypeDescriptor(VectorizationResourceTypeNames.IndexingProfiles)
            }
        };

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, CONTENT_SOURCE_PROFILES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, CONTENT_SOURCE_PROFILES_FILE_PATH, default);
                var contentSourceProfilesStore = JsonSerializer.Deserialize<ProfileStore<ContentSourceProfile>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _contentSourceProfiles = contentSourceProfilesStore!.ToDictionary();
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, TEXT_PARTITIONING_PROFILES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, TEXT_PARTITIONING_PROFILES_FILE_PATH, default);
                var textPartitionProfileStore = JsonSerializer.Deserialize<ProfileStore<TextPartitioningProfile>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _textPartitioningProfiles = textPartitionProfileStore!.ToDictionary();
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, TEXT_EMBEDDING_PROFILES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, TEXT_EMBEDDING_PROFILES_FILE_PATH, default);
                var textEmbeddingProfileStore = JsonSerializer.Deserialize<ProfileStore<TextEmbeddingProfile>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _textEmbeddingProfiles = textEmbeddingProfileStore!.ToDictionary();
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, INDEXING_PROFILES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, INDEXING_PROFILES_FILE_PATH, default);
                var indexingProfileStore = JsonSerializer.Deserialize<ProfileStore<IndexingProfile>>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _indexingProfiles = indexingProfileStore!.ToDictionary();
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(List<ResourceTypeInstance> instances) where T: class =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => GetContentSourceProfile<T>(instances),
                VectorizationResourceTypeNames.TextPartitioningProfiles => GetTextPartitioningProfile<T>(instances),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => GetTextEmbeddingProfile<T>(instances),
                VectorizationResourceTypeNames.IndexingProfiles => GetIndexingProfile<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };

        #region Helpers for GetResourceInternal<T>

        private T GetContentSourceProfile<T>(List<ResourceTypeInstance> instances) where T: class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(ContentSourceProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _contentSourceProfiles.TryGetValue(instances[0].ResourceId!, out var contentSource);
            return contentSource as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetTextPartitioningProfile<T>(List<ResourceTypeInstance> instances) where T: class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextPartitioningProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _textPartitioningProfiles.TryGetValue(instances[0].ResourceId!, out var textPartitioningProfile);
            return textPartitioningProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetTextEmbeddingProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(TextEmbeddingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _textEmbeddingProfiles.TryGetValue(instances[0].ResourceId!, out var textEmbeddingProfile);
            return textEmbeddingProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        private T GetIndexingProfile<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(IndexingProfile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _indexingProfiles.TryGetValue(instances[0].ResourceId!, out var indexingProfile);
            return indexingProfile as T
                ?? throw new ResourceProviderException($"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource)
        {
            switch (instances[0].ResourceType)
            {
                case VectorizationResourceTypeNames.ContentSourceProfiles:
                    await UpdateProfile<ContentSourceProfile>(instances, serializedResource, _contentSourceProfiles, CONTENT_SOURCE_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextPartitioningProfiles:
                    await UpdateProfile<TextPartitioningProfile>(instances, serializedResource, _textPartitioningProfiles, TEXT_PARTITIONING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.TextEmbeddingProfiles:
                    await UpdateProfile<TextEmbeddingProfile>(instances, serializedResource, _textEmbeddingProfiles, TEXT_EMBEDDING_PROFILES_FILE_PATH);
                    break;
                case VectorizationResourceTypeNames.IndexingProfiles:
                    await UpdateProfile<IndexingProfile>(instances, serializedResource, _indexingProfiles, INDEXING_PROFILES_FILE_PATH);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.");
            }
        }

        #region Helpers for UpsertResourceAsync

        private async Task UpdateProfile<T>(List<ResourceTypeInstance> instances, string serializedProfile, Dictionary<string, T> profileStore, string storagePath)
            where T : VectorizationProfileBase
        {
            var profile = JsonSerializer.Deserialize<T>(serializedProfile)
                ?? throw new ResourceProviderException("The object definition is invalid.");
            profile.ObjectId = GetObjectId(instances);

            if (instances[0].ResourceId != profile.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).");

            profileStore[profile.Name] = profile;

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    storagePath,
                    JsonSerializer.Serialize(ProfileStore<T>.FromDictionary(profileStore)),
                    default,
                    default);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task UpsertResourceAsync<T>(List<ResourceTypeInstance> instances, T resource)
        {
            switch (instances[0].ResourceType)
            {
                case VectorizationResourceTypeNames.VectorizationRequests:
                    await UpdateVectorizationRequest(instances, resource as VectorizationRequest ??
                        throw new ResourceProviderException($"The type {typeof(T)} was not VectorizationRequest."));
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.");
            }
        }

        #region Helpers for UpsertResourceAsync<T>

        private async Task UpdateVectorizationRequest(List<ResourceTypeInstance> instances, VectorizationRequest request)
        {
            request.ObjectId = GetObjectId(instances);
            await Task.CompletedTask;
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<string> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances) =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => LoadAndSerializeProfiles<ContentSourceProfile>(instances[0], _contentSourceProfiles),
                VectorizationResourceTypeNames.TextPartitioningProfiles => LoadAndSerializeProfiles<TextPartitioningProfile>(instances[0], _textPartitioningProfiles),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => LoadAndSerializeProfiles<TextEmbeddingProfile>(instances[0], _textEmbeddingProfiles),
                VectorizationResourceTypeNames.IndexingProfiles => LoadAndSerializeProfiles<IndexingProfile>(instances[0], _indexingProfiles),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for GetResourcesAsyncInternal

        private string LoadAndSerializeProfiles<T>(ResourceTypeInstance instance, Dictionary<string, T> profileStore)
        {
            if (instance.ResourceId == null)
            {
                var serializedProfiles = new List<string>();

                foreach (var profile in profileStore.Values)
                {
                    serializedProfiles.Add(
                        JsonSerializer.Serialize<T>(profile));
                }

                return $"[{string.Join(",", [.. serializedProfiles])}]";
            }
            else
            {
                if (!profileStore.TryGetValue(instance.ResourceId, out var profile))
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} agent resource.");

                return JsonSerializer.Serialize<T>(profile);
            }
        }

        #endregion
    }
}
