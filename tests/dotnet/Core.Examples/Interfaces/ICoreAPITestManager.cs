using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Core.Examples.Interfaces;

/// <summary>
/// Provides methods to manage calls to the Core API's endpoints.
/// </summary>
public interface ICoreAPITestManager
{
    /// <summary>
    /// Creates and renames a session.
    /// </summary>
    /// <returns>Returns the new Session ID.</returns>
    /// <exception cref="FoundationaLLMException"></exception>
    Task<string> CreateSessionAsync();

    /// <summary>
    /// Sends a user prompt to the specified agent within the specified session.
    /// </summary>
    /// <param name="completionRequestest"></param>
    /// <returns>Returns a completion response.</returns>
    /// <exception cref="FoundationaLLMException"></exception>
    Task<Completion> SendSessionCompletionRequestAsync(CompletionRequest completionRequest);

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
    /// Sends a user prompt to the specified agent. Also considered a "sessionless" request.
    /// </summary>
    /// <param name="completionRequest"></param>
    /// <returns></returns>
    Task<Completion> SendOrchestrationCompletionRequestAsync(CompletionRequest completionRequest);

    /// <summary>
    /// Deletes a chat session.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task DeleteSessionAsync(string sessionId);
}