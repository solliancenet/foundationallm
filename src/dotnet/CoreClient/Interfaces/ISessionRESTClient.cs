using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's session endpoints.
    /// </summary>
    public interface ISessionRESTClient
    {
        /// <summary>
        /// Retrieves all chat sessions.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Session>> GetAllChatSessionsAsync();

        /// <summary>
        /// Sets the rating for a message.
        /// </summary>
        /// <param name="sessionId">The chat session ID that contains the message to rate.</param>
        /// <param name="messageId">The ID of the message to rate.</param>
        /// <param name="rating">Set to true for a positive rating and false for a negative rating.</param>
        /// <returns></returns>
        Task RateMessageAsync(string sessionId, string messageId, bool rating);

        /// <summary>
        /// Creates and renames a session.
        /// </summary>
        /// <returns>Returns the new Session ID.</returns>
        Task<string> CreateSessionAsync();

        /// <summary>
        /// Renames a chat session.
        /// </summary>
        /// <param name="sessionId">The chat session ID.</param>
        /// <param name="sessionName">The new session name.</param>
        /// <returns></returns>
        Task<string> RenameChatSession(string sessionId, string sessionName);

        /// <summary>
        /// Gets a completion prompt by session ID and completion prompt ID.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="completionPromptId"></param>
        /// <returns></returns>
        Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId);

        /// <summary>
        /// Returns the chat messages related to an existing session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId);

        /// <summary>
        /// Deletes a chat session.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task DeleteSessionAsync(string sessionId);
    }
}
