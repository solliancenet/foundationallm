using Asp.Versioning;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/orchestration")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public OrchestrationController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpPost("complete")]
        public async Task<SemanticKernelCompletionResponse> Complete([FromBody] SemanticKernelCompletionRequest request)
        {
            var info = await _semanticKernelService.Complete(request.Prompt, request.MessageHistory);

            return new SemanticKernelCompletionResponse() { Info = info };
        }

        [HttpPost("summarize")]
        public async Task<SemanticKernelSummarizeResponse> Summarize([FromBody] SemanticKernelSummarizeRequest request)
        {
            var info = await _semanticKernelService.Summarize(request.Prompt);

            return new SemanticKernelSummarizeResponse() { Info = info };
        }

        [HttpPost("memory/add")]
        public async Task AddMemory()
        {
            //await _semanticKernelService.AddMemory();

            throw new NotImplementedException();
        }

        [HttpDelete("memory/remove")]
        public async Task RemoveMemory()
        {
            //await _semanticKernelService.RemoveMemory();

            throw new NotImplementedException();
        }
    }
}
