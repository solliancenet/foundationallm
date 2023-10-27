using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Chat;

/// <summary>
/// The session object.
/// </summary>
public record Session
{
    /// <summary>
    /// The unique identifier.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// The type of the session.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The Partition key.
    /// </summary>
    public string SessionId { get; set; }
    /// <summary>
    /// The number of tokens used in the session.
    /// </summary>
    public int? TokensUsed { get; set; }
    /// <summary>
    /// The name of the session.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The list of messages associated with the session.
    /// </summary>
    [JsonIgnore]
    public List<Message> Messages { get; set; }

    /// <summary>
    /// Constructor for Session.
    /// </summary>
    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = Id;
        TokensUsed = 0;
        Name = "New Chat";
        Messages = new List<Message>();
    }

    /// <summary>
    /// Adds a message to the list of messages associated with the session.
    /// </summary>
    /// <param name="message">The message to be added.</param>
    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

    /// <summary>
    /// Updates an existing message in the list of messages associated with the session.
    /// </summary>
    /// <param name="message">The updated message.</param>
    public void UpdateMessage(Message message)
    {
        var match = Messages.Single(m => m.Id == message.Id);
        var index = Messages.IndexOf(match);
        Messages[index] = message;
    }
}