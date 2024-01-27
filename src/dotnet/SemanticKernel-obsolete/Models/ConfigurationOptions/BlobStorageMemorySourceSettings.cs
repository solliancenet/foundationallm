using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides configuration options for a Blob Storage service.
    /// </summary>
    public record BlobStorageMemorySourceSettings : BlobStorageSettings
    {
        /// <summary>
        /// The configurable file path.
        /// </summary>
        public required string ConfigFilePath { get; init; }
    }
}
