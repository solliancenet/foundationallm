namespace FoundationaLLM.SemanticKernel.Core.Interfaces;

/// <summary>
/// Interface for a System Prompt service.
/// </summary>
public interface ISystemPromptService
{
    /// <summary>
    /// Gets the specified system prompt.
    /// </summary>
    /// <param name="promptName">The system prompt name.</param>
    /// <param name="forceRefresh">The flag that inform the System Prompt service to do a cache refresh.</param>
    /// <returns>The system prompt text.</returns>
    Task<string> GetPrompt(string promptName, bool forceRefresh = false);
}
