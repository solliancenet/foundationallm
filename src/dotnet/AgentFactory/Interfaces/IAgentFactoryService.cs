using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IAgentFactoryService
{
    /// <summary>
    /// Status value to return when the APIs status endpoint is called.
    /// </summary>
    string Status { get; }

    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);

    /// <summary>
    /// Retrieve a summarization for the passed in prompt from the orchestration service.
    /// </summary>
    Task<SummaryResponse> GetSummary(SummaryRequest content);
}
