using FoundationaLLM.Vectorization.Services.ContentSources;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Models the content of the content source store managed by the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class ContentSourceStore
    {
        /// <summary>
        /// The list of all content sources that are registered for use by the vectorization pipelines.
        /// </summary>
        public required List<ContentSourceProfile> ContentSourceProfiles { get; set; }
    }
}
