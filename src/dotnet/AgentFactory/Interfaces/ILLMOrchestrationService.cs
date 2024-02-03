using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
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
        /// <param name="request">Hub populated request object containing agent, prompt, language model, and data source information</param>
        /// <returns></returns>
        Task<LLMOrchestrationCompletionResponse> GetCompletion(LLMOrchestrationCompletionRequest request);

        /// <summary>
        /// Method for retrieving a string summarizing text passed into the function.
        /// </summary>
        /// <param name="orchestrationRequest">TThe orchestration request that includes the text to summarize.</param>
        /// <returns>Returns a string containing the summary.</returns>
        Task<string> GetSummary(LLMOrchestrationRequest orchestrationRequest);

        Task<LLMOrchestrationCompletionResponse> GetCompletion(string agentName, string serializedRequest);
    }
}
