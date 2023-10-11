using Asp.Versioning;
using FoundationaLLM.Common.Controllers;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace FoundationaLLM.Core.API.Controllers
{
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : APIControllerBase
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
