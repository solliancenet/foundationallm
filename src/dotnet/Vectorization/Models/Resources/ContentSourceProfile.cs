using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Provides details about a content source.
    /// </summary>
    public class ContentSourceProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the content source.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required ContentSourceType Type { get; set; }
    }
}
