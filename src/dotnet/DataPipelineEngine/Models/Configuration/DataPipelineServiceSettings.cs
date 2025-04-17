using FoundationaLLM.Common.Models.Configuration.CosmosDB;

namespace FoundationaLLM.DataPipelineEngine.Models.Configuration
{
    /// <summary>
    /// Provides the settings for the Data Pipeline service.
    /// </summary>
    public class DataPipelineServiceSettings
    {
        /// <summary>
        /// Gets or sets the Azure Cosmos DB settings.
        /// </summary>
        public required AzureCosmosDBSettings CosmosDB { get; set; }
    }
}
