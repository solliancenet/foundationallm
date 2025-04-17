using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Provides settings for the FoundationaLLM Context API file service.
    /// </summary>
    public class FileServiceSettings
    {
        /// <summary>
        /// Gets or sets the storage settings.
        /// </summary>
        public required BlobStorageServiceSettings Storage { get; set; }

        /// <summary>
        /// Gets or sets the Azure Cosmos DB settings.
        /// </summary>
        public required AzureCosmosDBSettings CosmosDB { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated list of file extensions that are subject to knowledge search.
        /// </summary>
        public required string KnowledgeSearchFileExtensions { get; set; }
    }
}
