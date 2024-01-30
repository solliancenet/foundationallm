using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// Data Source base class.
    /// </summary>
    public class DataSourceBase : MetadataBase
    {
        /// <summary>
        /// Descriptor for the type of data in the data source.
        /// </summary>
        /// <example>Survey data for a CSV file that contains survey results.</example>
        [JsonProperty("data_description")]
        public string? DataDescription { get; set; }
    }
}
