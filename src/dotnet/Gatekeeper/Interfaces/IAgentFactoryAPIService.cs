using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IAgentFactoryAPIService
{
    Task<CompletionResponseBase> GetCompletion(CompletionRequestBase completionRequest);
    Task<SummarizeResponseBase> GetSummary(SummarizeRequestBase content);
    Task<bool> SetLLMOrchestrationPreference(string orchestrationService);
}