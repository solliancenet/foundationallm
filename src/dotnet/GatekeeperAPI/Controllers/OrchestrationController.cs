using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    /// <summary>
    /// Wrapper for Gatekeeper service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Gatekeeper API orchestration controller.
    /// </remarks>
    /// <param name="gatekeeperService"></param>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController(
        IGatekeeperService gatekeeperService) : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService = gatekeeperService;

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest) =>
            await _gatekeeperService.GetCompletion(completionRequest);

        /// <summary>
        /// Gets a summary from the Gatekeeper service.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        [HttpPost("summary")]
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest) =>
            await _gatekeeperService.GetSummary(summaryRequest);
    }
}
