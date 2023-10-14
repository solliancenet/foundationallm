using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubMessage
    {
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }
        [JsonPropertyName("user_context")]
        public string? UserContext { get; set; }

    }
}
