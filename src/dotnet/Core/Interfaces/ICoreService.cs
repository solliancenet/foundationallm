using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Search;

namespace FoundationaLLM.Core.Interfaces;

public interface ICoreService
{
    string Status { get; }

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
    /// Receive a prompt from a user, vectorize it from the OpenAI service, and get a completion from the OpenAI service.
    /// </summary>
    Task<Completion> GetChatCompletionAsync(string? sessionId, string userPrompt);

    /// <summary>
    /// Generate a name for a chat message, based on the passed in prompt.
    /// </summary>
    Task<Completion> SummarizeChatSessionNameAsync(string? sessionId, string prompt);

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training, discoverability, and other benefits down the road.
    /// </summary>
    Task<Message> RateMessageAsync(string id, string sessionId, bool? rating);

    Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId);
}