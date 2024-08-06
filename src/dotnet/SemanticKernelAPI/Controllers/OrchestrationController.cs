using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    /// <summary>
    /// Wrapper for the Semantic Kernel service.
    /// </summary>
    /// <param name="semanticKernelService">The Semantic Kernel service handling requests.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class OrchestrationController(
        ISemanticKernelService semanticKernelService) : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService = semanticKernelService;

        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">The <see cref="LLMCompletionRequest"/> completion request.</param>
        /// <returns>A <see cref="LLMCompletionResponse"/> containing the response to the completion request.</returns>
        [HttpPost("completion")]
        public async Task<LLMCompletionResponse> GetCompletion([FromBody] LLMCompletionRequest request) =>
            await _semanticKernelService.GetCompletion(request);
    }
}
