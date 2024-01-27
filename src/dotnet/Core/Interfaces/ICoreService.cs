using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Search;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Contains methods for managing chat sessions and messages, and for getting completions from the
/// orchestrator.
/// </summary>
public interface ICoreService
{
    /// <summary>
    /// Returns list of chat session ids and names.
    /// </summary>
    Task<List<Session>> GetAllChatSessionsAsync();

    /// <summary>
    /// Returns the chat messages related to an existing session.
    /// </summary>
    Task<List<Message>> GetChatSessionMessagesAsync(string sessionId);

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    Task<Session> CreateNewChatSessionAsync();

    /// <summary>
    /// Rename the chat session from its default (eg., "New Chat") to the summary provided by OpenAI.
    /// </summary>
    Task<Session> RenameChatSessionAsync(string sessionId, string newChatSessionName);

    /// <summary>
    /// Delete a chat session and related messages.
    /// </summary>
    Task DeleteChatSessionAsync(string sessionId);

    /// <summary>
    /// Receive a prompt from a user, retrieve the message history from the related session,
    /// generate a completion response, and log full completion results.
    /// </summary>
    Task<Completion> GetChatCompletionAsync(string? sessionId, string userPrompt);

    /// <summary>
    /// Provides a completion for a user prompt, without a session.
    /// </summary>
    Task<Completion> GetCompletionAsync(DirectCompletionRequest directCompletionRequest);

    /// <summary>
    /// Generate a name for a chat message, based on the passed in prompt.
    /// </summary>
    Task<Completion> SummarizeChatSessionNameAsync(string? sessionId, string prompt);

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training, discoverability, and other benefits down the road.
    /// </summary>
    Task<Message> RateMessageAsync(string id, string sessionId, bool? rating);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId);
}
