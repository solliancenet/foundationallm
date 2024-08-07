using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides details about a text embedding profile.
    /// </summary>
    public class TextEmbeddingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the text splitter.
        /// </summary>
        [JsonPropertyName("text_embedding")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TextEmbeddingType TextEmbedding { get; set; }

        /// <summary>
        /// The object id of the AI model used for embedding.
        /// Not required for Gateway embedding.
        /// </summary>
        [JsonPropertyName("embedding_ai_model_object_id")]
        public string? EmbeddingAIModelObjectId { get; set; }
    }
}
