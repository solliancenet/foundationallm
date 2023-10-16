using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService;

        public OrchestrationController(
            IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            return await _gatekeeperService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            return await _gatekeeperService.GetSummary(summaryRequest);
        }

        [HttpPost("preference")]
        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            return await _gatekeeperService.SetLLMOrchestrationPreference(orchestrationService);
        }
    }
}