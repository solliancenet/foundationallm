using Asp.Versioning;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/refinement")]
    public class RefinementController : ControllerBase
    {
        private readonly IRefinementService _gatekeeperService;

        public RefinementController(
            IRefinementService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        [HttpPost("refine")]
        public async Task RefineUserPrompt([FromBody] string userPrompt)
        {
            await _gatekeeperService.RefineUserPrompt(userPrompt);
        }
    }
}