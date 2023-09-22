using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernelAPI.Controllers
{
    //[Authorize]
    [Route("api/semantickernnel")]
    [ApiController]
    public class SemanticKernnelController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public SemanticKernnelController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _semanticKernelService.Test();

            return new OkResult();
        }
    }
}
