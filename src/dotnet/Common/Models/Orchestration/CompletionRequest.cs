using FoundationaLLM.Common.Models.Chat;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Orchestration;

public class CompletionRequest : OrchestrationRequest
{
    [JsonProperty("user_context")]
    public string UserContext { get; init; }

    [JsonProperty("message_history")]
    public List<MessageHistoryItem>? MessageHistory { get; init; } = new List<MessageHistoryItem>();
}
