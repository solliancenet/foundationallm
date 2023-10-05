using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Models.Orchestration;

public class CompletionRequestBase
{
    public string Prompt { get; init; }
    public List<MessageHistoryItem> MessageHistory { get; init; }
}
