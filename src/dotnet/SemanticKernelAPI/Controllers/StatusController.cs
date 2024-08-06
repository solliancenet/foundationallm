using Asp.Versioning;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Semantic Kernel API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get() => new OkObjectResult(new ServiceStatusInfo
        {
            Name = ServiceNames.SemanticKernelAPI,
            InstanceName = ValidatedEnvironment.MachineName,
            Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
            Status = ServiceStatuses.Ready
        });

        /// <summary>
        /// Returns the allowed HTTP methods for the Semantic Kernel API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", new[] { "GET", "POST", "OPTIONS", "DELETE" });

            return Ok();
        }
    }
}
