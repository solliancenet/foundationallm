using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Core.Interfaces
{
    public interface ILLMOrchestrationService
    {
        bool IsInitialized { get; }

        Task<(string Completion, string UserPrompt, int UserPromptTokens, int ResponseTokens, float[]? UserPromptEmbedding)> GetResponse(string userPrompt, List<MessageHistory> messageHistory);

        Task<string> Summarize(string content);
    }
}
