using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubRequest
    {
        [JsonProperty("user_prompt")]
        public string UserPrompt { get; set; }

        [JsonProperty("user_context")]
        public string? UserContext { get; set; }

    }
}
