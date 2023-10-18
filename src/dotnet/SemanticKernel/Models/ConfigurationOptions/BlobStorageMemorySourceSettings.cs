namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides configuration options for a Blob Storage service.
    /// </summary>
    public record BlobStorageMemorySourceSettings
    {
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
