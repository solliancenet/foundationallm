using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// SQL Database data source metadata model.
    /// </summary>
    public class SQLDatabaseDataSource: MetadataBase
    {
        /// <summary>
        /// SQL Database configuration settings.
        /// </summary>
        [JsonProperty("configuration")]
        public SQLDatabaseConfiguration? Configuration { get; set; }
    }
}
