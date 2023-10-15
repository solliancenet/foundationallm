using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record DataSourceHubRequest
    {
        [JsonProperty("data_sources")]
        public List<string>? DataSources { get; set; }

    }
}
