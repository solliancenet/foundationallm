using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Models.Orchestration.SemanticKernel;

public class SemanticKernelCompletionRequest : CompletionRequestBase
{
    public List<MessageHistory> MessageHistory { get; init; } 
}
