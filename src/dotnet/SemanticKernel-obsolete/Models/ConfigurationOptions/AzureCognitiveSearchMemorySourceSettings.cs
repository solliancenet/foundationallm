using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions
{

    /// <summary>
    /// Provides configuration options for an Azure Cognitive Search memory source service.
    /// </summary>
    public record AzureCognitiveSearchMemorySourceSettings : BlobStorageSettings
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
        /// The configurable file path.
        /// </summary>
        public required string ConfigFilePath { get; init; }
    }
}
