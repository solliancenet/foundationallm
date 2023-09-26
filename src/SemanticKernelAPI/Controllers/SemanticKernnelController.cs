using Asp.Versioning;
using FoundationaLLM.SemanticKernelAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernelAPI.Controllers
{
    [Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/semantickernnel")]
    public class SemanticKernnelController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public SemanticKernnelController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpGet("test")]
        public async Task Test()
        {
            await _semanticKernelService.Test();
        }
    }
}
