using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Provides details about a text partitioning profile.
    /// </summary>
    public class TextPartitioningProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the text splitter.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TextSplitterType TextSplitter { get; set; }
    }
}
