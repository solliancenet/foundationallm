using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.ChatAPI.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IChatService chatService,
            ILogger<StatusController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet(Name = "GetServiceStatus")]
        public string Get()
        {
            return _chatService.Status;
        }
    }
}
