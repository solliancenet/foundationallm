using Asp.Versioning;
using Azure.AI.OpenAI;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ICoreService coreService,
            ILogger<SessionsController> logger)
        {
            _coreService = coreService;
            _logger = logger;
        }

        [HttpGet(Name = "GetAllChatSessions")]
        public async Task<IEnumerable<Session>> GetAllChatSessions()
        {
            return await _coreService.GetAllChatSessionsAsync();
        }

        [HttpGet("{sessionId}/messages", Name = "GetChatSessionMessages")]
        public async Task<IEnumerable<Message>> GetChatSessionMessages(string sessionId)
        {
            return await _coreService.GetChatSessionMessagesAsync(sessionId);
        }

        [HttpPost("{sessionId}/message/{messageId}/rate", Name = "RateMessage")]
        public async Task RateMessage(string messageId, string sessionId, bool? rating)
        {
            await _coreService.RateMessageAsync(messageId, sessionId, rating);
        }

        [HttpGet("{sessionId}/completionprompts/{completionPromptId}", Name = "GetCompletionPrompt")]
        public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId)
        {
            return await _coreService.GetCompletionPrompt(sessionId, completionPromptId);
        }

        [HttpPost(Name = "CreateNewChatSession")]
        public async Task<Session> CreateNewChatSession()
        {
            return await _coreService.CreateNewChatSessionAsync();
        }

        [HttpPost("{sessionId}/rename", Name = "RenameChatSession")]
        public async Task RenameChatSession(string sessionId, string newChatSessionName)
        {
            await _coreService.RenameChatSessionAsync(sessionId, newChatSessionName);
        }

        [HttpDelete("{sessionId}", Name = "DeleteChatSession")]
        public async Task DeleteChatSession(string sessionId)
        {
            await _coreService.DeleteChatSessionAsync(sessionId);
        }

        [HttpPost("{sessionId}/completion", Name = "GetChatCompletion")]
        public async Task<Completion> GetChatCompletion(string sessionId, [FromBody] string userPrompt)
        {
            return await _coreService.GetChatCompletionAsync(sessionId, userPrompt);
        }

        [HttpPost("{sessionId}/summarize-name", Name = "SummarizeChatSessionName")]
        public async Task<Completion> SummarizeChatSessionName(string sessionId, [FromBody] string prompt)
        {
            return await _coreService.SummarizeChatSessionNameAsync(sessionId, prompt);
        }
    }
}
