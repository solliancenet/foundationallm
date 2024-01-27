using Asp.Versioning;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Methods for orchestration services exposed by the Gatekeeper API service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Orchestration Controller.
    /// </remarks>
    /// <param name="coreService">The Core service provides methods for getting
    /// completions from the orchestrator.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="OrchestrationController"/> type name.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController(
        ICoreService coreService,
        ILogger<OrchestrationController> logger) : ControllerBase
    {
        private readonly ICoreService _coreService = coreService;
#pragma warning disable IDE0052 // Remove unread private members.
        private readonly ILogger<OrchestrationController> _logger = logger;

        /// <summary>
        /// Requests a completion from the downstream APIs.
        /// </summary>
        /// <param name="directCompletionRequest">The user prompt for which to generate a completion.</param>
        [HttpPost("completion", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion([FromBody] DirectCompletionRequest directCompletionRequest)
        {
            var completionResponse = await _coreService.GetCompletionAsync(directCompletionRequest);

            return Ok(completionResponse);
        }
    }
}
