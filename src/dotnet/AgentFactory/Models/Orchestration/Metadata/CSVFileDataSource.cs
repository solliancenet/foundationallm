using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// CSV file data source metadata model.
    /// </summary>
    public class CSVFileDataSource
    {
        /// <summary>
        /// CSV file configuration settings metadata.
        /// </summary>
        [JsonProperty("configuration")]
        public CSVFileConfiguration Configuration { get; set; }
    }
}
