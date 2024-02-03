using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Provides details about an indexing profile.
    /// </summary>
    public class IndexingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the indexer.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required IndexerType Indexer { get; set; }
    }
}
