using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    /// <summary>
    /// Wrapper for Gatekeeper service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService;

        /// <summary>
        /// Constructor for the Gatekeeper API orchestration controller.
        /// </summary>
        /// <param name="gatekeeperService"></param>
        public OrchestrationController(
            IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            return await _gatekeeperService.GetCompletion(completionRequest);
        }

        /// <summary>
        /// Gets a summary from the Gatekeeper service.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        [HttpPost("summary")]
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            return await _gatekeeperService.GetSummary(summaryRequest);
        }
    }
}