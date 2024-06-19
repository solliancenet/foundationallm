using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Methods for managing vectorization requests.
    /// </summary>
    /// <remarks>
    /// Constructor for the vectorization request controller.
    /// </remarks>
    /// <param name="vectorizationRequestProcessor">The vectorization request processor.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class VectorizationRequestController(
        IVectorizationRequestProcessor vectorizationRequestProcessor) : ControllerBase
    {
        /// <summary>
        /// Handles an incoming vectorization request by starting a new vectorization pipeline.
        /// </summary>
        /// <param name="vectorizationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessRequest([FromBody] VectorizationRequest vectorizationRequest)
            => new OkObjectResult(await vectorizationRequestProcessor.ProcessRequest(vectorizationRequest));

    }
}
