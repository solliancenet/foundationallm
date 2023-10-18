using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

/// <summary>
/// Interface for calling the Agent Factory API.
/// </summary>
public interface IAgentFactoryAPIService
{
    /// <summary>
    /// Gets a completion from the Agent Factory API.
    /// </summary>
    /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
    /// <returns>The completion response.</returns>
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);

    /// <summary>
    /// Gets a summary from the Agent Factory API.
    /// </summary>
    /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
    /// <returns>The summary response.</returns>
    Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest);

    /// <summary>
    /// Sets the preffered orchestration service.
    /// </summary>
    /// <param name="orchestrationService">The name of the preferred orchestration service.</param>
    /// <returns>True if the preffered orchestration service was set. Otherwise, returns False.</returns>
    Task<bool> SetLLMOrchestrationPreference(string orchestrationService);
}