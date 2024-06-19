using FoundationaLLM.Common.Models.Chat;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// The completion request object.
/// </summary>
public class CompletionRequest
{
    /// <summary>
    /// The message history associated with the completion request.
    /// </summary>
    [JsonPropertyName("message_history")]
    public List<MessageHistoryItem>? MessageHistory { get; set; } = [];

    /// <summary>
    /// The session ID.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Represent the input or user prompt.
    /// </summary>
    [JsonPropertyName("user_prompt")]
    public required string UserPrompt { get; set; }

    /// <summary>
    /// The name of the selected agent.
    /// </summary>
    [JsonPropertyName("agent_name")]
    public string? AgentName { get; set; }

    /// <summary>
    /// One or more attachments to include with the orchestration request.
    /// The values should be the ObjectID of the attachment(s).
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; set; }

    /// <summary>
    /// A list of Gatekeeper feature names used by the orchestration request.
    /// </summary>
    [JsonPropertyName("gatekeeper_options")]
    public string[]? GatekeeperOptions { get; set; }

    /// <summary>
    /// Collection of model settings to override with the orchestration request.
    /// </summary>
    [JsonPropertyName("settings")]
    public OrchestrationSettings? Settings { get; set; }
}
