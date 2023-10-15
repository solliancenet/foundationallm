using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record DataSourceHubRequest
    {
        [JsonPropertyName("data_sources")]
        public List<string>? DataSources { get; set; }

    }
}
