namespace FoundationaLLM.SemanticKernelAPI.Core.Interfaces;

public interface ISystemPromptService
{
    Task<string> GetPrompt(string promptName, bool forceRefresh = false);
}
