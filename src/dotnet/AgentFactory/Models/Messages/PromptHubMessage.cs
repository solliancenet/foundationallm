using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record PromptHubMessage
    {
        [JsonPropertyName("agent_name")]
        public string? AgentName { get; set; }
    }
}
