namespace FoundationaLLM.SemanticKernel.Core.Interfaces;

public interface ISystemPromptService
{
    Task<string> GetPrompt(string promptName, bool forceRefresh = false);
}
