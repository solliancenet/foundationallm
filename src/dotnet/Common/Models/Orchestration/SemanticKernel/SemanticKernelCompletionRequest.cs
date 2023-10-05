using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Models.Orchestration.SemanticKernel;

public class SemanticKernelCompletionRequest : CompletionRequestBase
{
    public List<MessageHistoryItem> MessageHistory { get; init; } 
}
