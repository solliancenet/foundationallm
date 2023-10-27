using Azure.Search.Documents.Indexes;

namespace FoundationaLLM.Common.Models.Chat;

/// <summary>
/// The message object.
/// </summary>
public record Message
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    [SearchableField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; }
    /// <summary>
    /// The type of the message.
    /// </summary>
    [SimpleField]
    public string Type { get; set; }
    /// <summary>
    /// The Partition key.
    /// </summary>
    [SimpleField]
    public string SessionId { get; set; }
    /// <summary>
    /// The timestamp when the message was created.
    /// </summary>
    [SimpleField]
    public DateTime TimeStamp { get; set; }
    /// <summary>
    /// The sender of the message.
    /// </summary>
    [SimpleField]
    public string Sender { get; set; }
    /// <summary>
    /// The number of tokens associated with the message, if any.
    /// </summary>
    [SimpleField]
    public int? Tokens { get; set; }
    /// <summary>
    /// The text content of the message.
    /// </summary>
    [SimpleField]
    public string Text { get; set; }
    /// <summary>
    /// The rating associated with the message, if any.
    /// </summary>
    [SimpleField]
    public bool? Rating { get; set; }
    /// <summary>
    /// The vector associated with the message.
    /// </summary>
    [FieldBuilderIgnore]
    public float[]? Vector { get; set; }
    /// <summary>
    /// The identifier for the completion prompt associated with the message.
    /// </summary>
    public string? CompletionPromptId { get; set; }

    /// <summary>
    /// Constructor for Message.
    /// </summary>
    public Message(string sessionId, string sender, int? tokens, string text, float[]? vector, bool? rating)
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Message);
        SessionId = sessionId;
        Sender = sender;
        Tokens = tokens ?? 0;
        TimeStamp = DateTime.UtcNow;
        Text = text;
        Rating = rating;
        Vector = vector;
    }
}