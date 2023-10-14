using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record DataSourceHubMessage
    {
        [JsonPropertyName("data_source_name")]
        public string? DataSourceName { get; set; }

    }
}
