using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Chat.Helpers;

public interface IChatManager
{
    /// <summary>
    /// Returns list of chat session ids and names for left-hand nav to bind to (display Name and ChatSessionId as hidden).
    /// </summary>
    Task<List<Session>> GetAllChatSessionsAsync();

    /// <summary>
    /// Returns the chat messages to display on the main web page when the user selects a chat from the left-hand nav.
    /// </summary>
    Task<List<Message>> GetChatSessionMessagesAsync(string sessionId);

    /// <summary>
    /// User creates a new Chat Session.
    /// </summary>
    Task CreateNewChatSessionAsync();

    /// <summary>
    /// Rename the chat session.
    /// </summary>
    /// <param name="sessionId">The id of the session to rename.</param>
    /// <param name="newChatSessionName">The new name for the session.</param>
    /// <param name="onlyUpdateLocalSessionsCollection">If true, only update the local sessions collection.</param>
    Task RenameChatSessionAsync(string sessionId, string newChatSessionName, bool onlyUpdateLocalSessionsCollection = false);

    /// <summary>
    /// User deletes a chat session and related messages.
    /// </summary>
    /// <param name="sessionId">The id of the session to delete.</param>
    Task DeleteChatSessionAsync(string sessionId);

    /// <summary>
    /// Receive a prompt from a user, vectorize it, and get a completion from the orchestration service.
    /// </summary>
    /// <param name="sessionId">The id of the session for which to get a completion.</param>
    /// <param name="userPrompt">The prompt to send to the orchestration service.</param>
    Task<string> GetChatCompletionAsync(string sessionId, string userPrompt);

    /// <summary>
    /// Generate a name for a chat message, based on the passed in prompt.
    /// </summary>
    /// <param name="sessionId">The id of the session for which to generate a name.</param>
    /// <param name="prompt">The prompt to use to generate the name.</param>
    Task<string> SummarizeChatSessionNameAsync(string sessionId, string prompt);

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training,
    /// discoverability, and other benefits down the road.
    /// </summary>
    /// <param name="id">The id of the message to rate.</param>
    /// <param name="sessionId">The id of the session to which the message belongs.</param>
    /// <param name="rating">The rating to assign to the message.</param>
    Task<Message> RateMessageAsync(string id, string sessionId, bool? rating);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId);
}