using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IAgentFactoryService
{
    /// <summary>
    /// Property for setting the preferred orchestration engine.
    /// </summary>
    /// <param name="orchestrationService">The name of the preferred orchestration service.</param>
    /// <returns>Return true if the preferred service is set. Otherwise, returns false.</returns>
    bool SetLLMOrchestrationPreference(string orchestrationService);

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
