using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace FoundationaLLM.Vectorization.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class VectorizationResourceProviderService(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)] IStorageService storageService,
        ILogger<VectorizationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            storageService,
            logger)
    {
        private Dictionary<string, ContentSourceProfile> _contentSourceProfiles = [];
        private Dictionary<string, TextPartitioningProfile> _textPartitioningProfiles = [];
        private Dictionary<string, TextEmbeddingProfile> _textEmbeddingProfiles = [];
        private Dictionary<string, IndexingProfile> _indexingProfiles = [];

        private const string CONTENT_SOURCE_PROFILES_FILE_NAME = "vectorization-content-source-profiles.json";
        private const string TEXT_PARTITION_PROFILES_FILE_NAME = "vectorization-text-partitioning-profiles.json";
        private const string TEXT_EMBEDDING_PROFILES_FILE_NAME = "vectorization-text-embedding-profiles.json";
        private const string INDEXING_PROFILES_FILE_NAME = "vectorization-indexing-profiles.json";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Vectorization;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> _resourceTypes => new Dictionary<string, ResourceTypeDescriptor>
        {
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

            var contentSourceProfilesFilePath = $"/{_name}/{CONTENT_SOURCE_PROFILES_FILE_NAME}";
            var partitionProfilesFilePath = $"/{_name}/{TEXT_PARTITION_PROFILES_FILE_NAME}";
            var embeddingProfilesPath = $"/{_name}/{TEXT_EMBEDDING_PROFILES_FILE_NAME}";
            var indexingProfilesPath = $"/{_name}/{INDEXING_PROFILES_FILE_NAME}";

            if (await _storageService.FileExistsAsync(_storageContainerName, contentSourceProfilesFilePath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, contentSourceProfilesFilePath, default);
                var contentSourceProfilesStore = JsonConvert.DeserializeObject<ContentSourceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _contentSourceProfiles = contentSourceProfilesStore!.ContentSourceProfiles.ToDictionary(cs => cs.Name);
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, partitionProfilesFilePath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, partitionProfilesFilePath, default);
                var textPartitionProfileStore = JsonConvert.DeserializeObject<TextPartitioningProfileStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _textPartitioningProfiles = textPartitionProfileStore!.TextPartitioningProfiles.ToDictionary(tpp => tpp.Name);
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, embeddingProfilesPath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, embeddingProfilesPath, default);
                var textEmbeddingProfileStore = JsonConvert.DeserializeObject<TextEmbeddingProfileStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _textEmbeddingProfiles = textEmbeddingProfileStore!.TextEmbeddingProfiles.ToDictionary(tep => tep.Name);
            }

            if (await _storageService.FileExistsAsync(_storageContainerName, indexingProfilesPath, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, indexingProfilesPath, default);
                var indexingProfileStore = JsonConvert.DeserializeObject<IndexingProfileStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _indexingProfiles = indexingProfileStore!.IndexingProfiles.ToDictionary(ip => ip.Name);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(List<ResourceTypeInstance> instances) where T: class =>
            instances[0].ResourceType switch
            {
                VectorizationResourceTypeNames.ContentSourceProfiles => GetContentSourceProfiles<T>(instances),
                VectorizationResourceTypeNames.TextPartitioningProfiles => GetTextPartitioningProfile<T>(instances),
                VectorizationResourceTypeNames.TextEmbeddingProfiles => GetTextEmbeddingProfile<T>(instances),
                VectorizationResourceTypeNames.IndexingProfiles => GetIndexingProfile<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };

        private T GetContentSourceProfiles<T>(List<ResourceTypeInstance> instances) where T: class
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
    }
}
