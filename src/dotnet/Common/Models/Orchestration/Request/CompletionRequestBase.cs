using FoundationaLLM.Common.Models.Conversation;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// Base for completion requests
    /// </summary>
    public class CompletionRequestBase
    {
        /// <summary>
        /// The Operation ID identifying the completion request.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// Indicates whether this is a long-running operation.
        /// </summary>
        [JsonPropertyName("long_running_operation")]
        public bool LongRunningOperation { get; set; }

        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Represent the input or user prompt.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public required string UserPrompt { get; init; }

        /// <summary>
        /// The message history associated with the completion request.
        /// </summary>
        [JsonPropertyName("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; set; } = [];
    }
}
