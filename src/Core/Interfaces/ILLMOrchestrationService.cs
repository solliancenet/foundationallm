using Solliance.AICopilot.Core.Models.Chat;

namespace Solliance.AICopilot.Core.Interfaces
{
    public interface ILLMOrchestrationService
    {
        bool IsInitialized { get; }

        Task<(string Completion, string UserPrompt, int UserPromptTokens, int ResponseTokens, float[]? UserPromptEmbedding)> GetResponse(string userPrompt, List<Message> messageHistory);

        Task<string> Summarize(string content);
    }
}
