using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Contains constants of the keys for all keyed dependency injections.
    /// </summary>
    public static class DependencyInjectionKeys
    {
        /// <summary>
        /// The dependency injection key for the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProviderService = "FoundationaLLM:Vectorization:ResourceProviderService";

        /// <summary>
        /// The dependency injection key for the blob storage vectorization state service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_BlobStorageVectorizationStateService = "FoundationaLLM:Vectorization:BlobStorageVectorizationStateService";

        /// <summary>
        /// The dependency injection key for the vectorization data lake content source service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_DataLakeContentSourceService = "FoundationaLLM:Vectorization:DataLakeContentSourceService";

        /// <summary>
        /// The dependency injection key for the content source service factory.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceServiceFactory = "FoundationaLLM:Vectorization:ContentSourceServiceFactory";

        /// <summary>
        /// The dependency injection key for the Semantic Kernel text embedding service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService";

        /// <summary>
        /// The dependency injection key for the Azure AI Search indexing service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureAISearchIndexingService = "FoundationaLLM:Vectorization:AzureAISearchIndexingService";

        /// <summary>
        /// The dependency injection key for the vectorization queues configuration section.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues = "FoundationaLLM:Vectorization:Queues";

        /// <summary>
        /// The dependency injection key for the vectorization steps configuration section.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Steps = "FoundationaLLM:Vectorization:Steps";
        /// <summary>
        /// The dependency injection key for the FoundationaLLM.Agent resource provider.
        /// </summary>
        public const string FoundationaLLM_Agent_ResourceProviderService = "FoundationaLLM:Agent:ResourceProviderService";
        /// <summary>
        /// The dependency injection key for the FoundationaLLM.Prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_Prompt_ResourceProviderService = "FoundationaLLM:Prompt:ResourceProviderService";
    }
}
