using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Represents a request to create a code session.
    /// </summary>
    public class CreateCodeSessionRequest
    {
        /// <summary>
        /// Gets or sets the name of the agent for which the code session is created.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public required string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the conversation.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        public required string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the context in which the code session is created.
        /// </summary>
        [JsonPropertyName("context")]
        public required string Context { get; set; }

        /// <summary>
        /// Gets or sets the name of the code session provider.
        /// </summary>
        [JsonPropertyName("endpoint_provider")]
        public required string EndpointProvider { get; set; }

        /// <summary>
        /// Gets or sets the language of the code session.
        /// </summary>
        [JsonPropertyName("language")]
        public required string Language { get; set; }
    }
}
