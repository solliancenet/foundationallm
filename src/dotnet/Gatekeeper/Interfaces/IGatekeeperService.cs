using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

/// <summary>
/// Interface for the Gatekeeper service.
/// </summary>
public interface IGatekeeperService
{
    /// <summary>
    /// Gets a completion from the Gatekeeper service.
    /// </summary>
    /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
    /// <returns>The completion response.</returns>
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);

    /// <summary>
    /// Gets a summary from the Gatekeeper service.
    /// </summary>
    /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
    /// <returns>The summary response.</returns>
    Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest);
}
