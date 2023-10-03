using Asp.Versioning;
using Azure.AI.OpenAI;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.ChatAPI.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(IChatService chatService,
            ILogger<SessionsController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet(Name = "GetAllChatSessions")]
        public async Task<IEnumerable<Session>> GetAllChatSessions()
        {
            return await _chatService.GetAllChatSessionsAsync();
        }

        [HttpGet("{sessionId}/messages", Name = "GetChatSessionMessages")]
        public async Task<IEnumerable<Message>> GetChatSessionMessages(string sessionId)
        {
            return await _chatService.GetChatSessionMessagesAsync(sessionId);
        }

        [HttpPost("{sessionId}/message/{messageId}/rate", Name = "RateMessage")]
        public async Task RateMessage(string messageId, string sessionId, bool? rating)
        {
            await _chatService.RateMessageAsync(messageId, sessionId, rating);
        }

        [HttpGet("{sessionId}/completionprompts/{completionPromptId}", Name = "GetCompletionPrompt")]
        public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId)
        {
            return await _chatService.GetCompletionPrompt(sessionId, completionPromptId);
        }

        [HttpPost(Name = "CreateNewChatSession")]
        public async Task<Session> CreateNewChatSession()
        {
            return await _chatService.CreateNewChatSessionAsync();
        }

        [HttpPost("{sessionId}/rename", Name = "RenameChatSession")]
        public async Task RenameChatSession(string sessionId, string newChatSessionName)
        {
            await _chatService.RenameChatSessionAsync(sessionId, newChatSessionName);
        }

        [HttpDelete("{sessionId}", Name = "DeleteChatSession")]
        public async Task DeleteChatSession(string sessionId)
        {
            await _chatService.DeleteChatSessionAsync(sessionId);
        }

        [HttpPost("{sessionId}/completion", Name = "GetChatCompletion")]
        public async Task<Completion> GetChatCompletion(string sessionId, [FromBody] string userPrompt)
        {
            return await _chatService.GetChatCompletionAsync(sessionId, userPrompt);
        }

        [HttpPost("{sessionId}/summarize-name", Name = "SummarizeChatSessionName")]
        public async Task<Completion> SummarizeChatSessionName(string sessionId, [FromBody] string prompt)
        {
            return await _chatService.SummarizeChatSessionNameAsync(sessionId, prompt);
        }
    }
}
