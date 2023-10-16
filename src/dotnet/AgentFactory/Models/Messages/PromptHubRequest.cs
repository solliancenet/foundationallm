using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record PromptHubRequest
    {
        [JsonProperty("agent_name")]
        public string? AgentName { get; set; }
    }
}
