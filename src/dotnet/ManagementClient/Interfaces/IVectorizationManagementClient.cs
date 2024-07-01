using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage vectorization resources.
    /// </summary>
    public interface IVectorizationManagementClient
    {
        /// <summary>
        /// Retrieves all vectorization pipelines.
        /// </summary>
        /// <returns>All vectorization pipelines to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<VectorizationPipeline>>> GetVectorizationPipelinesAsync();

        /// <summary>
        /// Retrieves a specific vectorization pipeline by name.
        /// </summary>
        /// <param name="name">The name of the vectorization pipeline to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<VectorizationPipeline>> GetVectorizationPipelineAsync(string name);

        /// <summary>
        /// Retrieves all text partitioning profiles.
        /// </summary>
        /// <returns>All text partitioning profiles to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<TextPartitioningProfile>>> GetTextPartitioningProfilesAsync();

        /// <summary>
        /// Retrieves a specific text partitioning profile by name.
        /// </summary>
        /// <param name="name">The name of the text partitioning profile to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<TextPartitioningProfile>> GetTextPartitioningProfileAsync(string name);

        /// <summary>
        /// Retrieves all text embedding profiles.
        /// </summary>
        /// <returns>All text embedding profiles to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<TextEmbeddingProfile>>> GetTextEmbeddingProfilesAsync();

        /// <summary>
        /// Retrieves a specific text embedding profile by name.
        /// </summary>
        /// <param name="name">The name of the text embedding profile to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<TextEmbeddingProfile>> GetTextEmbeddingProfileAsync(string name);

        /// <summary>
        /// Retrieves all indexing profiles.
        /// </summary>
        /// <returns>All indexing profiles to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<IndexingProfile>>> GetIndexingProfilesAsync();

        /// <summary>
        /// Retrieves a specific indexing profile by name.
        /// </summary>
        /// <param name="name">The name of the indexing profile to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<IndexingProfile>> GetIndexingProfileAsync(string name);

        /// <summary>
        /// Activates a vectorization pipeline.
        /// </summary>
        /// <param name="pipelineName">The vectorization pipeline to activate.</param>
        /// <returns></returns>
        Task<VectorizationResult> ActivateVectorizationPipelineAsync(string pipelineName);

        /// <summary>
        /// Deactivates a vectorization pipeline.
        /// </summary>
        /// <param name="pipelineName">The vectorization pipeline to deactivate.</param>
        /// <returns></returns>
        Task<VectorizationResult> DeactivateVectorizationPipelineAsync(string pipelineName);

        /// <summary>
        /// Purges a deleted vectorization pipeline by its name. This action is irreversible.
        /// </summary>
        /// <param name="pipelineName">The name of the vectorization pipeline to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeVectorizationPipelineAsync(string pipelineName);

        /// <summary>
        /// Purges a deleted text partitioning profile by its name. This action is irreversible.
        /// </summary>
        /// <param name="textPartitioningProfileName">The name of the text partitioning profile to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeTextPartitioningProfileAsync(string textPartitioningProfileName);

        /// <summary>
        /// Purges a deleted text embedding profile by its name. This action is irreversible.
        /// </summary>
        /// <param name="textEmbeddingProfileName">The name of the text embedding profile to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeTextEmbeddingProfileAsync(string textEmbeddingProfileName);

        /// <summary>
        /// Checks the availability of a resource name for an indexing profile. If the name is available, the
        /// <see cref="ResourceNameCheckResult.Status"/> value will be "Allowed". If the name is
        /// not available, the <see cref="ResourceNameCheckResult.Status"/> value will be "Denied" and
        /// the <see cref="ResourceNameCheckResult.Message"/> will explain the reason why. Typically,
        /// a denied name is due to a name conflict with an existing indexing profile or an indexing profile that was
        /// deleted but not purged.
        /// </summary>
        /// <param name="resourceName">Contains the name of the resource to check.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the required properties within the argument
        /// are empty or missing.</exception>
        Task<ResourceNameCheckResult> CheckIndexingProfileNameAsync(ResourceName resourceName);

        /// <summary>
        /// Returns indexing profiles that match the filter criteria.
        /// </summary>
        /// <param name="resourceFilter">The filter criteria to apply to the request.</param>
        /// <returns></returns>
        Task<List<IndexingProfile>> FilterIndexingProfileAsync(ResourceFilter resourceFilter);

        /// <summary>
        /// Purges a deleted indexing profile by its name. This action is irreversible.
        /// </summary>
        /// <param name="indexingProfileName">The name of the indexing profile to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeIndexingProfileAsync(string indexingProfileName);

        /// <summary>
        /// Upserts a vectorization pipeline resource. If a vectorization pipeline does not exist, it will be created.
        /// If a vectorization pipeline does exist, it will be updated.
        /// </summary>
        /// <param name="resource">The resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertVectorizationPipelineAsync(VectorizationPipeline resource);

        /// <summary>
        /// Upserts a text partitioning profile resource. If a text partitioning profile does not exist, it will be created.
        /// If a text partitioning profile does exist, it will be updated.
        /// </summary>
        /// <param name="resource">The resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertTextPartitioningProfileAsync(TextPartitioningProfile resource);

        /// <summary>
        /// Upserts a text embedding profile resource. If a text embedding profile does not exist, it will be created.
        /// If a text embedding profile does exist, it will be updated.
        /// </summary>
        /// <param name="resource">The resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertTextEmbeddingProfileAsync(TextEmbeddingProfile resource);

        /// <summary>
        /// Upserts an indexing profile resource. If an indexing profile does not exist, it will be created.
        /// If an indexing profile does exist, it will be updated.
        /// </summary>
        /// <param name="resource">The resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertIndexingProfileAsync(IndexingProfile resource);

        /// <summary>
        /// Deletes a vectorization pipeline resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgeVectorizationPipelineAsync"/> method with the same name.
        /// </summary>
        /// <param name="name">The name of the vectorization pipeline resource to delete.</param>
        /// <returns></returns>
        Task DeleteVectorizationPipelineAsync(string name);

        /// <summary>
        /// Deletes a text partitioning profile resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgeTextPartitioningProfileAsync"/> method with the same name.
        /// </summary>
        /// <param name="name">The name of the text partitioning profile resource to delete.</param>
        /// <returns></returns>
        Task DeleteTextPartitioningProfileAsync(string name);

        /// <summary>
        /// Deletes a text embedding profile resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgeTextEmbeddingProfileAsync"/> method with the same name.
        /// </summary>
        /// <param name="name">The name of the text embedding profile resource to delete.</param>
        /// <returns></returns>
        Task DeleteTextEmbeddingProfileAsync(string name);

        /// <summary>
        /// Deletes an indexing profile resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgeIndexingProfileAsync"/> method with the same name.
        /// </summary>
        /// <param name="name">The name of the indexing profile resource to delete.</param>
        /// <returns></returns>
        Task DeleteIndexingProfileAsync(string name);
    }
}
