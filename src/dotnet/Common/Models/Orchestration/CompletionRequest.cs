using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Models.Orchestration;

public class CompletionRequest
{
    public string Prompt { get; init; }

    //public string UserContext { get; init; }

    public List<MessageHistoryItem> MessageHistory { get; init; }
}
