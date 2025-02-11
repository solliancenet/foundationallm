using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Response;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation;

/// <summary>
/// The message object.
/// </summary>
public record Message
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The type of the message.
    /// </summary>
    public string Type { get; set; } = nameof(Message);

    /// <summary>
    /// The Partition key.
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// The optional identifier of the long-running operation.
    /// </summary>
    /// <remarks>
    /// The operation id will be set only for messages that are part of a long-running operation.
    /// </remarks>
    [JsonPropertyName("operation_id")]
    public string? OperationId { get; set; }

    /// <summary>
    /// The timestamp when the message was created.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The sender of the message.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// The name of the message sender, such as the signed-in user or the name of the agent.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// The display name of the message sender, if applicable, such as the agent's display name.
    /// </summary>
    public string? SenderDisplayName { get; set; }

    /// <summary>
    /// The number of tokens associated with the message, if any.
    /// </summary>
    public int Tokens { get; set; }

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The optional rewrite of the text content of the message.
    /// </summary>
    public string? TextRewrite { get; set; }

    /// <summary>
    /// The rating associated with the message, if any.
    /// </summary>
    public bool? Rating { get; set; }

    /// <summary>
    /// The comments associated with the rating.
    /// </summary>
    public string? RatingComments { get; set; }

    /// <summary>
    /// The UPN of the user who created the chat session.
    /// </summary>
    public string UPN { get; set; }

    /// <summary>
    /// Deleted flag used for soft delete.
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// The vector associated with the message.
    /// </summary>
    public float[]? Vector { get; set; }

    /// <summary>
    /// The identifier for the completion prompt associated with the message.
    /// </summary>
    public string? CompletionPromptId { get; set; }

    /// <summary>
    /// Stores the expected completion for the message and used for evaluating the actual vs. expected agent completion.
    /// This should be stored in the agent response.
    /// </summary>
    public string? ExpectedCompletion { get; set; }

    /// <summary>
    /// The sources associated with the completion prompt.
    /// </summary>
    public ContentArtifact[]? ContentArtifacts { get; set; }

    /// <summary>
    /// A list of results from the analysis.
    /// </summary>
    public List<AnalysisResult>? AnalysisResults { get; set; }

    /// <summary>
    /// One or more attachments included with the orchestration request.
    /// The values should be the Object ID of the attachment(s).
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<string>? Attachments { get; init; }

    /// <summary>
    /// Contains the details of the attachments. This is not stored in the database.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<AttachmentDetail>? AttachmentDetails { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public List<MessageContent>? Content { get; set; }

    /// <summary>
    /// The status of the long-running operation.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
}
