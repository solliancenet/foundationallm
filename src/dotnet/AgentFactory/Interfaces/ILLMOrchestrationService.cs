using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Interfaces
{
    /// <summary>
    /// LLM Orchestration Service interface
    /// </summary>
    public interface ILLMOrchestrationService
    {
        /// <summary>
        /// Flag indicating if the orchestration service has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Method for retrieving a completion from the orchestration service.
        /// </summary>
        /// <param name="userPrompt">The user propt for which a completion should be retrieved.</param>
        /// <param name="messageHistory">List of previous user prompts in the form of a message history.</param>
        /// <returns>Returns a Completion response.</returns>
        Task<CompletionResponse> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory);

        /// <summary>
        /// Method for retrieving a string summarizing text passed into the function.
        /// </summary>
        /// <param name="content">The text to summarize.</param>
        /// <returns>Returns a string containing the summary.</returns>
        Task<string> GetSummary(string content);
    }
}
