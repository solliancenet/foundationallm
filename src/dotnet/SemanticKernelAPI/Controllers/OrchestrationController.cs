using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    /// <summary>
    /// Wrapper for the Semantic Kernel service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        /// <summary>
        /// Constructor for the Semantic Kernel API orchestration controller.
        /// </summary>
        /// <param name="semanticKernelService"></param>
        public OrchestrationController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest request)
        {
            var completionResponse = await _semanticKernelService.GetCompletion(request.UserPrompt, request.MessageHistory ?? new List<MessageHistoryItem>());

            return new CompletionResponse() { Completion = completionResponse };
        }

        /// <summary>
        /// Gets a summary from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        [HttpPost("summary")]
        public async Task<SummaryResponse> GetSummary([FromBody] SummaryRequest request)
        {
            var info = await _semanticKernelService.GetSummary(request.UserPrompt);

            return new SummaryResponse() { Summary = info };
        }

        /// <summary>
        /// Add an object instance and its associated vectorization to the memory store.
        /// </summary>
        /// <returns></returns>
        [HttpPost("memory/add")]
        public Task AddMemory()
        {
            //await _semanticKernelService.AddMemory();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes an object instance and its associated vectorization from the memory store.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("memory/remove")]
        public Task RemoveMemory()
        {
            //await _semanticKernelService.RemoveMemory();

            throw new NotImplementedException();
        }
    }
}
