using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a reference to an index entry that contains vectorized content.
    /// </summary>
    public class VectorizationIndexReference
    {
        /// <summary>
        /// The unique identifier of the index entry reference.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("type")]
        public required string IndexEntryId { get; set; }

        /// <summary>
        /// The position of the index entry reference.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("position")]
        public required int Position { get; set; }
    }
}
