using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models
{
    /// <summary>
    /// Encapsulates a message sent to or received from AzureML.
    /// </summary>
    public class AzureMLChatMessage
    {
        /// <summary>
        /// The role of the message. "system", "user" or "assistant".
        /// </summary>
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        /// <summary>
        /// The text of the message.
        /// </summary>
        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }
}
