using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Orchestration.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    /// <param name="orchestrationService">The <see cref="IOrchestrationService"/> that provides orchestration capabilities.</param>
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class StatusController(
        IOrchestrationService orchestrationService) : ControllerBase
    {
        private readonly IOrchestrationService _orchestrationService = orchestrationService;

        /// <summary>
        /// Returns the status of the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpGet(Name = "GetServiceStatus")]
        public async Task<ServiceStatusInfo> Get(string instanceId) =>
            await _orchestrationService.GetStatus(instanceId);

        /// <summary>
        /// Returns the allowed HTTP methods for the Orchestration API service.
        /// </summary>
        [AllowAnonymous]
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET", "POST", "OPTIONS" });

            return Ok();
        }
    }
}
