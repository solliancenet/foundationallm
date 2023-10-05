using Asp.Versioning;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IAgentFactoryService _agentFactoryService;

        public OrchestrationController(
            IAgentFactoryService agentFactoryService)
        {
            _agentFactoryService = agentFactoryService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponseBase> GetCompletion([FromBody] CompletionRequestBase completionRequest)
        {
            return await _agentFactoryService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<string> GetSummary([FromBody] string content)
        {
            return await _agentFactoryService.GetSummary(content);
        }
    }
}
