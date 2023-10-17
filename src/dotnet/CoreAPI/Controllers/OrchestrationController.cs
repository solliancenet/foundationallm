using Asp.Versioning;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace FoundationaLLM.Core.API.Controllers
{
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
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

        [AllowAnonymous]
        [HttpPost("completion", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion(CompletionRequest completionRequest)
        {
            var completionResponse = await _gatekeeperAPIService.GetCompletion(completionRequest);

            return Ok(completionResponse);
        }

        [AllowAnonymous]
        [HttpPost("summary", Name = "GetSummary")]
        public async Task<IActionResult> GetSummary(SummaryRequest summaryRequest)
        {
            var summaryResponse = await _gatekeeperAPIService.GetSummary(summaryRequest.Prompt);

            return Ok(summaryResponse);
        }
    }
}
