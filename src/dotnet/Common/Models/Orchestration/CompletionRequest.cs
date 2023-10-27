using FoundationaLLM.Common.Models.Chat;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// The completion request object.
/// </summary>
public class CompletionRequest : OrchestrationRequest
{
    /// <summary>
    /// The message history associated with the completion request.
    /// </summary>
    [JsonProperty("message_history")]
    public List<MessageHistoryItem>? MessageHistory { get; init; } = new List<MessageHistoryItem>();
}
