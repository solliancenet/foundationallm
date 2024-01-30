using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Methods for managing vectorization requests.
    /// </summary>
    /// <remarks>
    /// Constructor for the vectorization request controller.
    /// </remarks>
    /// <param name="vectorizationService"></param>
    [ApiVersion(1.0)]
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class VectorizationRequestController(
        IVectorizationService vectorizationService) : ControllerBase
    {
        readonly IVectorizationService _vectorizationService = vectorizationService;

        /// <summary>
        /// Handles an incoming vectorization request by starting a new vectorization pipeline.
        /// </summary>
        /// <param name="vectorizationRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessRequest([FromBody] VectorizationRequest vectorizationRequest) =>
            new OkObjectResult(await _vectorizationService.ProcessRequest(vectorizationRequest));
    }
}
