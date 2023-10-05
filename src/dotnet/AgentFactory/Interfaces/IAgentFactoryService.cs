using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;

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
    Task<SummarizeResponseBase> GetSummary(SummarizeRequestBase content);
}
