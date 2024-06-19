using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for interacting with the Core service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Core Controller.
    /// </remarks>
    /// <param name="coreService">The Core service provides methods for managing chat
    /// sessions and messages, and for getting completions from the orchestrator.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="SessionsController"/> type name.</param>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("[controller]")]
    public class SessionsController(ICoreService coreService,
        ILogger<SessionsController> logger) : ControllerBase
    {
        private readonly ICoreService _coreService = coreService;
        private readonly ILogger<SessionsController> _logger = logger;

        /// <summary>
        /// Returns list of chat session ids and names.
        /// </summary>
        [HttpGet(Name = "GetAllChatSessions")]
        public async Task<IEnumerable<Session>> GetAllChatSessions() =>
            await _coreService.GetAllChatSessionsAsync();

        /// <summary>
        /// Returns the chat messages related to an existing session.
        /// </summary>
        /// <param name="sessionId">The id of the session for which to retrieve chat messages.</param>
        [HttpGet("{sessionId}/messages", Name = "GetChatSessionMessages")]
        public async Task<IEnumerable<Message>> GetChatSessionMessages(string sessionId) =>
            await _coreService.GetChatSessionMessagesAsync(sessionId);

        /// <summary>
        /// Rate an assistant message. This can be used to discover useful AI responses for training,
        /// discoverability, and other benefits down the road.
        /// </summary>
        /// <param name="messageId">The id of the message to rate.</param>
        /// <param name="sessionId">The id of the session to which the message belongs.</param>
        /// <param name="rating">The rating to assign to the message.</param>
        [HttpPost("{sessionId}/message/{messageId}/rate", Name = "RateMessage")]
        public async Task<Message> RateMessage(string messageId, string sessionId, bool? rating) =>
            await _coreService.RateMessageAsync(messageId, sessionId, rating);

        /// <summary>
        /// Returns the completion prompt for a given session and completion prompt id.
        /// </summary>
        /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
        /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
        /// <returns></returns>
        [HttpGet("{sessionId}/completionprompts/{completionPromptId}", Name = "GetCompletionPrompt")]
        public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId) =>
            await _coreService.GetCompletionPrompt(sessionId, completionPromptId);

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        [HttpPost(Name = "CreateNewChatSession")]
        public async Task<Session> CreateNewChatSession() =>
            await _coreService.CreateNewChatSessionAsync();

        /// <summary>
        /// Rename the chat session.
        /// </summary>
        /// <param name="sessionId">The id of the session to rename.</param>
        /// <param name="newChatSessionName">The new name for the session.</param>
        [HttpPost("{sessionId}/rename", Name = "RenameChatSession")]
        public async Task<Session> RenameChatSession(string sessionId, string newChatSessionName) =>
            await _coreService.RenameChatSessionAsync(sessionId, newChatSessionName);

        /// <summary>
        /// Delete a chat session and related messages.
        /// </summary>
        [HttpDelete("{sessionId}", Name = "DeleteChatSession")]
        public async Task DeleteChatSession(string sessionId) =>
            await _coreService.DeleteChatSessionAsync(sessionId);

        /// <summary>
        /// Generate a name for a chat message, based on the passed in prompt.
        /// </summary>
        /// <param name="sessionId">The id of the session for which to generate a name.</param>
        /// <param name="prompt">The prompt to use to generate the name.</param>
        [HttpPost("{sessionId}/summarize-name", Name = "SummarizeChatSessionName")]
        public async Task<Completion> SummarizeChatSessionName(string sessionId, [FromBody] string prompt) =>
            await _coreService.SummarizeChatSessionNameAsync(sessionId, prompt);
    }
}
