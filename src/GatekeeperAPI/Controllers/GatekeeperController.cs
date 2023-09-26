using Asp.Versioning;
using FoundationaLLM.GatekeeperAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernelAPI.Controllers
{
    [Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/gatekeeper")]
    public class GatekeeperController : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService;

        public GatekeeperController(
            IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        [HttpGet("test")]
        public async Task Test()
        {
            await _gatekeeperService.Test();
        }
    }
}