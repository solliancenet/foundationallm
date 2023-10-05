using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Interfaces
{
    public interface ILLMOrchestrationService
    {
        bool IsInitialized { get; }

        Task<CompletionResponseBase> GetResponse(string userPrompt, List<MessageHistory> messageHistory);

        Task<string> Summarize(string content);
    }
}
