namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions
{

    /// <summary>
    /// Provides configuration options for an Azure Cognitive Search memory source service.
    /// </summary>
    public record AzureCognitiveSearchMemorySourceSettings
    {
        /// <summary>
        /// The name of the Azure Cognitive Search index.
        /// </summary>
        public required string IndexName { get; init; }

        /// <summary>
        /// The Azure Cognitive Search endpoint.
        /// </summary>
        public required string Endpoint { get; init; }

        /// <summary>
        /// The Azure Cognitive Search key.
        /// </summary>
        public required string Key { get; init; }

        /// <summary>
        /// The name of the blob storage container.
        /// </summary>
        public required string ConfigBlobStorageContainer { get; init; }

        /// <summary>
        /// The connection string for the blob storage.
        /// </summary>
        public required string ConfigBlobStorageConnection { get; init; }

        /// <summary>
        /// The configurable file path.
        /// </summary>
        public required string ConfigFilePath { get; init; }
    }
}
