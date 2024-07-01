using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class VectorizationManagementClient(IManagementRESTClient managementRestClient) : IVectorizationManagementClient
    {
        #region Get Methods
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<VectorizationPipeline>>> GetVectorizationPipelinesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.VectorizationPipelines
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<VectorizationPipeline>> GetVectorizationPipelineAsync(string name)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{name}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"VectorizationPipeline '{name}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<TextPartitioningProfile>>> GetTextPartitioningProfilesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.TextPartitioningProfiles
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<TextPartitioningProfile>> GetTextPartitioningProfileAsync(string name)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{name}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"TextPartitioningProfile '{name}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<TextEmbeddingProfile>>> GetTextEmbeddingProfilesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.TextEmbeddingProfiles
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<TextEmbeddingProfile>> GetTextEmbeddingProfileAsync(string name)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{name}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"TextEmbeddingProfile '{name}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<IndexingProfile>>> GetIndexingProfilesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.IndexingProfiles
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<IndexingProfile>> GetIndexingProfileAsync(string name)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{name}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"IndexingProfile '{name}' not found.");
            }

            var resource = result[0];

            return resource;
        }
        #endregion Get Methods

        #region Actions
        /// <inheritdoc/>
        public async Task<VectorizationResult> ActivateVectorizationPipelineAsync(string pipelineName)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentException("Pipeline name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<VectorizationResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Activate}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<VectorizationResult> DeactivateVectorizationPipelineAsync(string pipelineName)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentException("Pipeline name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<VectorizationResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Deactivate}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeVectorizationPipelineAsync(string pipelineName)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentException("Pipeline name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        //public async Task<ResourceNameCheckResult> CheckTextPartitioningProfileNameAsync(ResourceName resourceName)
        //{
        //    if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
        //    {
        //        throw new ArgumentException("Resource name and type must be provided.");
        //    }

        //    return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
        //        ResourceProviderNames.FoundationaLLM_Vectorization,
        //        $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{VectorizationResourceProviderActions.CheckName}",
        //        resourceName
        //    );
        //}

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeTextPartitioningProfileAsync(string textPartitioningProfileName)
        {
            if (string.IsNullOrWhiteSpace(textPartitioningProfileName))
            {
                throw new ArgumentException("TextPartitioningProfile name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{textPartitioningProfileName}/{VectorizationResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        //public async Task<ResourceNameCheckResult> CheckTextEmbeddingProfileNameAsync(ResourceName resourceName)
        //{
        //    if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
        //    {
        //        throw new ArgumentException("Resource name and type must be provided.");
        //    }

        //    return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
        //        ResourceProviderNames.FoundationaLLM_Vectorization,
        //        $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{VectorizationResourceProviderActions.CheckName}",
        //        resourceName
        //    );
        //}

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeTextEmbeddingProfileAsync(string textEmbeddingProfileName)
        {
            if (string.IsNullOrWhiteSpace(textEmbeddingProfileName))
            {
                throw new ArgumentException("TextEmbeddingProfile name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{textEmbeddingProfileName}/{VectorizationResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceNameCheckResult> CheckIndexingProfileNameAsync(ResourceName resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
            {
                throw new ArgumentException("Resource name and type must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.CheckName}",
                resourceName
            );
        }

        /// <inheritdoc/>
        public async Task<List<IndexingProfile>> FilterIndexingProfileAsync(ResourceFilter resourceFilter) =>
            await managementRestClient.Resources.ExecuteResourceActionAsync<List<IndexingProfile>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.Filter}",
                resourceFilter
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeIndexingProfileAsync(string indexingProfileName)
        {
            if (string.IsNullOrWhiteSpace(indexingProfileName))
            {
                throw new ArgumentException("IndexingProfile name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{indexingProfileName}/{VectorizationResourceProviderActions.Purge}",
                new { }
            );
        }

        #endregion Actions

        #region Upsert Methods

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertVectorizationPipelineAsync(
            VectorizationPipeline resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("Resource must be provided.");
            }
            return await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{resource.Name}",
                resource
            );
        }


        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertTextPartitioningProfileAsync(
            TextPartitioningProfile resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("Resource must be provided.");
            }
            return await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{resource.Name}",
                resource
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertTextEmbeddingProfileAsync(
            TextEmbeddingProfile resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("Resource must be provided.");
            }
            return await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{resource.Name}",
                resource
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertIndexingProfileAsync(
            IndexingProfile resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("Resource must be provided.");
            }
            return await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{resource.Name}",
                resource
            );
        }

        #endregion Upsert Methods

        #region Delete Methods

        /// <inheritdoc/>
        public async Task DeleteVectorizationPipelineAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Pipeline name must be provided.");
            }
            await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{name}"
            );
        }

        /// <inheritdoc/>
        public async Task DeleteTextPartitioningProfileAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("TextPartitioningProfile name must be provided.");
            }
            await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{name}"
            );
        }

        /// <inheritdoc/>
        public async Task DeleteTextEmbeddingProfileAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("TextEmbeddingProfile name must be provided.");
            }
            await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{name}"
            );
        }

        /// <inheritdoc/>
        public async Task DeleteIndexingProfileAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("IndexingProfile name must be provided.");
            }
            await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{name}"
            );
        }

        #endregion Delete Methods
    }
}
