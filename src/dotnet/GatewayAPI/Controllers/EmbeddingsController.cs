using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gateway.API.Controllers
{
    /// <summary>
    /// Methods for managing embedding requests.
    /// </summary>
    /// <param name="gatewayCore">The <see cref="IGatewayCore"/> that provides LLM gateway services.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class EmbeddingsController(
        IGatewayCore gatewayCore)
    {
        readonly IGatewayCore _gatewayCore = gatewayCore;

        /// <summary>
        /// Handles an incoming text embedding request by starting a new embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="embeddingRequest">The <see cref="TextEmbeddingRequest"/> object with the details of the embedding request.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> StartEmbeddingOperation(string instanceId,
            [FromBody] TextEmbeddingRequest embeddingRequest) =>
            new OkObjectResult(await _gatewayCore.StartEmbeddingOperation(instanceId, embeddingRequest));

        /// <summary>
        /// Retrieves the outcome of a text embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The unique identifier of the text embedding operation.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> GetEmbeddingOperationResult(string instanceId, string operationId) =>
            new OkObjectResult(await _gatewayCore.GetEmbeddingOperationResult(instanceId, operationId));
    }
}
