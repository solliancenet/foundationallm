using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{

    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IGatekeeperAPIService _gatekeeperAPIService;
        private readonly ILogger<OrchestrationController> _logger;

        public OrchestrationController(IGatekeeperAPIService gatekeeperAPIService,
            ILogger<OrchestrationController> logger)
        {
            _gatekeeperAPIService = gatekeeperAPIService;
            _logger = logger;
        }

        [HttpPost(Name = "SetOrchestratorChoice")]
        public async Task<IActionResult> SetPreference([FromBody] string orchestrationService)
        {
            var orchestrationPreferenceSet = await _gatekeeperAPIService.SetLLMOrchestrationPreference(orchestrationService);

            if (orchestrationPreferenceSet)
            {
                return Ok();
            }

            _logger.LogError($"The LLM orchestrator {orchestrationService} is not supported.");
            return BadRequest($"The LLM orchestrator {orchestrationService} is not supported.");
        }
    }
}
