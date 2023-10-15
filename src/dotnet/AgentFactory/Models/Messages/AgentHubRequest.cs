using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubRequest
    {
        [JsonProperty("agent_name")]
        public string? AgentName { get; set; }

    }
}
