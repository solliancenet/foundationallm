using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Interfaces
{
    public interface ILLMOrchestrationService
    {
        bool IsInitialized { get; }

        Task<CompletionResponse> GetResponse(string userPrompt, List<MessageHistoryItem> messageHistory);

        Task<string> GetSummary(string content);
    }
}
