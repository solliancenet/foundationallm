using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Methods for managing vectorization requests.
    /// </summary>
    /// <param name="vectorizationRequestProcessor">The vectorization request processor.</param>
    /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
    /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
    /// <remarks>
    /// Constructor for the vectorization request controller.
    /// </remarks>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class VectorizationRequestController(
        ICallContext callContext,
        IVectorizationRequestProcessor vectorizationRequestProcessor) : ControllerBase
    {
        /// <summary>
        /// Handles an incoming vectorization request by starting a new vectorization pipeline.
        /// </summary>
        /// <param name="vectorizationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessRequest([FromBody] VectorizationRequest vectorizationRequest)
            => new OkObjectResult(await vectorizationRequestProcessor.ProcessRequest(vectorizationRequest, callContext.CurrentUserIdentity));

    }
}
