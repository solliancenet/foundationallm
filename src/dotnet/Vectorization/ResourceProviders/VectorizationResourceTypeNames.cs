using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.ResourceProviders
{
    /// <summary>
    /// Contains constants of the names of the resource types managed by the FoundationaLLM.Vectorization resource manager.
    /// </summary>
    public static class VectorizationResourceTypeNames
    {
        /// <summary>
        /// Vectorization requests.
        /// </summary>
        public const string VectorizationRequests = "vectorizationrequests";

        /// <summary>
        /// Vectorization content sources.
        /// </summary>
        public const string ContentSourceProfiles = "contentsourceprofiles";

        /// <summary>
        /// Text partitioning profiles.
        /// </summary>
        public const string TextPartitioningProfiles = "textpartitioningprofiles";

        /// <summary>
        /// Text embedding profiles.
        /// </summary>
        public const string TextEmbeddingProfiles = "textembeddingprofiles";

        /// <summary>
        /// Indexing profiles.
        /// </summary>
        public const string IndexingProfiles = "indexingprofiles";
    }
}
