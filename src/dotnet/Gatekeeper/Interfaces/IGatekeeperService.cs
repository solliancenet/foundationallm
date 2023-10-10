using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IGatekeeperService
{
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);
    Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest);
    Task<bool> SetLLMOrchestrationPreference(string orchestrationService);
}
