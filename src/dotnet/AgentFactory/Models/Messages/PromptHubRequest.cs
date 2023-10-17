using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// The format of a Prompt Hub request.
    /// </summary>
    public record PromptHubRequest
    {
        /// <summary>
        /// Gets the list of prompts for a target agent from the Prompt Hub.
        /// </summary>
        [JsonProperty("agent_name")]
        public string? AgentName { get; set; }
    }
}
