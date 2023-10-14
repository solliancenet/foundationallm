using Newtonsoft.Json;
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
