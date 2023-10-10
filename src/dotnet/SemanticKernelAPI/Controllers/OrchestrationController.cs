using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public OrchestrationController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest request)
        {
            var completionResponse = await _semanticKernelService.GetCompletion(request.Prompt, request.MessageHistory);

            return new CompletionResponse() { Completion = completionResponse };
        }

        [HttpPost("summary")]
        public async Task<SummaryResponse> GetSummary([FromBody] SummaryRequest request)
        {
            var info = await _semanticKernelService.GetSummary(request.Prompt);

            return new SummaryResponse() { Info = info };
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
