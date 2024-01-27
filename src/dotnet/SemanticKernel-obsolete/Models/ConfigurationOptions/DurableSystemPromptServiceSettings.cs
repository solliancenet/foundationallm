using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;

/// <summary>
/// Provides configuration options for the Durable System Prompt service.
/// </summary>
public record DurableSystemPromptServiceSettings : BlobStorageSettings
{
}
