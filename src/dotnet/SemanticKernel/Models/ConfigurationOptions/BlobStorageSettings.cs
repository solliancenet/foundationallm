namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;

public record BlobStorageSettings
{
    public required string BlobStorageContainer { get; set; }
    public required string BlobStorageConnection { get; set; }
}
