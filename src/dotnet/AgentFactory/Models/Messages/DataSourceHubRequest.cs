using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// The format of a Data Source Hub Request.
    /// </summary>
    public record DataSourceHubRequest
    {
        /// <summary>
        /// List of data sources to be returned from the Data Source Hub.
        /// </summary>
        [JsonProperty("data_sources")]
        public List<string>? DataSources { get; set; }

    }
}
