using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IAgentFactoryAPIService
{
    Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);
    Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest);
    Task<bool> SetLLMOrchestrationPreference(string orchestrationService);
}