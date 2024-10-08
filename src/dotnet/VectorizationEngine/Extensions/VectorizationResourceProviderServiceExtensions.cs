﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.ResourceProviders;

namespace FoundationaLLM.Vectorization.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="VectorizationResourceProviderService"/>.
    /// </summary>
    public static class VectorizationResourceProviderServiceExtensions
    {
        /// <summary>
        /// Retrieves all active vectorization pipelines.
        /// </summary>
        /// <param name="vectorizationResourceProvider">An instance of the vectorization resource provider service</param>
        /// <returns>List of active pipelines.</returns>
        public static async Task<List<VectorizationPipeline>> GetActivePipelines(this VectorizationResourceProviderService vectorizationResourceProvider)
        {
            var pipelinesList = await vectorizationResourceProvider.GetResourcesAsync($"/{VectorizationResourceTypeNames.VectorizationPipelines}") as List<ResourceProviderGetResult<VectorizationPipeline>>;
            if (pipelinesList == null)
                return [];
            return pipelinesList.Where(p => p.Resource.Active).Select(p => p.Resource).ToList();
        }

        /// <summary>
        /// Sets the specified vectorization pipeline to active or inactive.
        /// </summary>
        /// <param name="vectorizationResourceProvider">An instance of the vectorization resource provider.</param>
        /// <param name="pipelineObjectId">The object id of the pipeline to deactivate</param>
        /// <param name="activate">true if the pipeline should be activated, false if it is to be deactivated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns></returns>
        public static async Task TogglePipelineActivation(this VectorizationResourceProviderService vectorizationResourceProvider, string pipelineObjectId, bool activate, UnifiedUserIdentity userIdentity)
        {
            var pipeline =  await vectorizationResourceProvider.GetResourceAsync<VectorizationPipeline>(pipelineObjectId, userIdentity);                        
           
            if (pipeline == null || pipeline.Active == activate)
                // nothing to update
                return;

            // update the pipeline active state
            string slug = activate ? "activate" : "deactivate";           
            await vectorizationResourceProvider.ExecuteActionAsync($"{pipelineObjectId}/{slug}");
        }

        /// <summary>
        /// Retrieves the vectorization request resource with the specified name.
        /// </summary>
        /// <param name="vectorizationResourceProvider">An instance of the vectorization resource provider.</param>
        /// <param name="requestName">The name of the request to retrieve.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns>The vectorization request.</returns>
        public static async Task<VectorizationRequest> GetVectorizationRequestResource(this VectorizationResourceProviderService vectorizationResourceProvider, string requestName, UnifiedUserIdentity userIdentity)
            => await vectorizationResourceProvider.GetResourceAsync<VectorizationRequest>($"/{VectorizationResourceTypeNames.VectorizationRequests}/{requestName}", userIdentity);

    }
}
