using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// Represents a request that is sent to an AgentHub to get lists of agents.
    /// </summary>
    public record AgentHubRequest
    {
        /// <summary>
        /// The prompt that is being requested to be processed.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// The user context used to determine security and other agent selection logic.
        /// </summary>
        [JsonProperty("user_context")]
        public string? UserContext { get; set; }

    }
}
