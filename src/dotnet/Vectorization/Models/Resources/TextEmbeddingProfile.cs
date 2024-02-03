using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Provides details about a text embedding profile.
    /// </summary>
    public class TextEmbeddingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the text splitter.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TextEmbeddingType TextEmbedding { get; set; }
    }
}
