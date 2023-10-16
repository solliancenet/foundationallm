using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record PromptHubResponse
    {
        [JsonProperty("prompts")]
        public PromptMetadata[]? Prompts { get; set; }
    }

    public record PromptMetadata
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

    }
}
