using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

public interface IAgentFactoryService
{
    bool SetLLMOrchestrationPreference(string orchestrationService);
    string Status { get; }

    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    Task<CompletionResponseBase> GetCompletion(CompletionRequestBase completionRequest);

    /// <summary>
    /// Retrieve a summarization for the passed in prompt from the orchestration service.
    /// </summary>
    Task<string> GetSummary(string content);
}
