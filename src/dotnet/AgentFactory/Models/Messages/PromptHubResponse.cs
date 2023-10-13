using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record PromptHubResponse
    {
        [JsonPropertyName("prompts")]
        public PromptMetadata[]? Prompts { get; set; }
    }

    public record PromptMetadata
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

    }
}
