using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Messages
{
    /// <summary>
    /// The format of a Data Source Hub Request.
    /// </summary>
    public record DataSourceHubRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonProperty("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// List of data sources to be returned from the Data Source Hub.
        /// </summary>
        [JsonProperty("data_sources")]
        public List<string>? DataSources { get; set; }

    }
}
