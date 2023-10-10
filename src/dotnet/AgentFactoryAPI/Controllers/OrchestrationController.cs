using Asp.Versioning;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IAgentFactoryService _agentFactoryService;
        private readonly ILogger<OrchestrationController> _logger;

        public OrchestrationController(
            IAgentFactoryService agentFactoryService,
            ILogger<OrchestrationController> logger)
        {
            _agentFactoryService = agentFactoryService;
            _logger = logger;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest completionRequest)
        {
            return await _agentFactoryService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<SummaryResponse> GetSummary([FromBody] SummaryRequest content)
        {
            return await _agentFactoryService.GetSummary(content);
        }

        [HttpPost("preference", Name = "SetOrchestratorChoice")]
        public async Task<bool> SetPreference([FromBody] string orchestrationService)
        {
            var orchestrationPreferenceSet = _agentFactoryService.SetLLMOrchestrationPreference(orchestrationService);

            if (orchestrationPreferenceSet)
            {
                return true;
            }

            _logger.LogError($"The LLM orchestrator {orchestrationService} is not supported.");
            return false;
        }
    }
}
