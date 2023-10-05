using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Interfaces;

public interface IGatekeeperAPIService
{
    Task<CompletionResponseBase> GetCompletion(CompletionRequestBase completionRequest);
    Task<string> GetSummary(string content);
    Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);
    Task RemoveMemory(object item);
}