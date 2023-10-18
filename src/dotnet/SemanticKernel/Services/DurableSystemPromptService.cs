using Azure.Storage.Blobs;
using FoundationaLLM.SemanticKernel.Text;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.SemanticKernel.Core.Services;

/// <summary>
/// Implements the <see cref="ISystemPromptService"/> interface.
/// </summary>
public class DurableSystemPromptService : ISystemPromptService
{
    readonly DurableSystemPromptServiceSettings _settings;
    readonly BlobContainerClient _storageClient;
    Dictionary<string, string> _prompts = new Dictionary<string, string>();

    /// <summary>
    /// Constructor for the Durable System Prompt service.
    /// </summary>
    /// <param name="settings">The configuration options for the Durable System Prompt service.</param>
    public DurableSystemPromptService(
        IOptions<DurableSystemPromptServiceSettings> settings)
    {
        _settings = settings.Value;

        var blobServiceClient = new BlobServiceClient(_settings.BlobStorageConnection);
        _storageClient = blobServiceClient.GetBlobContainerClient(_settings.BlobStorageContainer);
    }

    /// <summary>
    /// Gets the specified system prompt.
    /// </summary>
    /// <param name="promptName">The system prompt name.</param>
    /// <param name="forceRefresh">The flag that inform the System Prompt service to do a cache refresh.</param>
    /// <returns>The system prompt text.</returns>
    public async Task<string> GetPrompt(string promptName, bool forceRefresh = false)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(promptName, nameof(promptName));

        if (_prompts.ContainsKey(promptName) && !forceRefresh)
            return _prompts[promptName];

        var blobClient = _storageClient.GetBlobClient(GetFilePath(promptName));
        var reader = new StreamReader(await blobClient.OpenReadAsync());
        var prompt = await reader.ReadToEndAsync();

        _prompts[promptName] = prompt.NormalizeLineEndings();

        return prompt;
    }

    /// <summary>
    /// Gets the file path of the system prompt.
    /// </summary>
    /// <param name="promptName">The system prompt name.</param>
    /// <returns>The file path of the system prompt.</returns>
    private string GetFilePath(string promptName)
    {
        var tokens = promptName.Split('.');

        var folderPath = (tokens.Length == 1 ? string.Empty : $"/{string.Join('/', tokens.Take(tokens.Length - 1))}");
        return $"{folderPath}/{tokens.Last()}.txt";
    }
}
