using Asp.Versioning;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    /// <summary>
    /// OrchestrationController class
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IAgentFactoryService _agentFactoryService;
        private readonly ILogger<OrchestrationController> _logger;


        /// <summary>
        /// Constructor for the Agent Factory orchestration controller
        /// </summary>
        /// <param name="agentFactoryService"></param>
        /// <param name="logger"></param>
        public OrchestrationController(
            IAgentFactoryService agentFactoryService,
            ILogger<OrchestrationController> logger)
        {
            _agentFactoryService = agentFactoryService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a completion from the Agent Factory
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest completionRequest)
        {
            return await _agentFactoryService.GetCompletion(completionRequest);
        }

        /// <summary>
        /// Gets a summary from the Agent Factory
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost("summary")]
        public async Task<SummaryResponse> GetSummary([FromBody] SummaryRequest content)
        {
            return await _agentFactoryService.GetSummary(content);
        }

        /// <summary>
        /// Sets the orchestration service to use for executing completion requests.
        /// </summary>
        /// <param name="orchestrationService">Name of the orchestration service to use.</param>
        /// <returns>Returns true if setting the preferred service was successful. Otherwise, returns false.</returns>
        [HttpPost("preference", Name = "SetOrchestratorChoice")]
        public bool SetPreference([FromBody] string orchestrationService)
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
