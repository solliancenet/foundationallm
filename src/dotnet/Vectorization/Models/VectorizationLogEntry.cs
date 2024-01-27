using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a log entry that contains information about vectorization operations.
    /// </summary>
    public class VectorizationLogEntry(string requestId, string messageId, string source, string text)
    {
        /// <summary>
        /// The unique identifier of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("rid")]
        public string RequestId { get; set; } = requestId;

        /// <summary>
        /// The identifier of underlying message retrieved from the request source.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("mid")]
        public string MessageId { get; set; } = messageId;

        /// <summary>
        /// The time at which the log entry was created.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("t")]
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The source of the log entry. This is usually the name of the vectorization step handler.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("src")]
        public string Source { get; set; } = source;

        /// <summary>
        /// The content of the log entry.
        /// </summary>
        [JsonPropertyOrder(4)]
        [JsonPropertyName("txt")]
        public string Text { get; set; } = text;
    }
}
