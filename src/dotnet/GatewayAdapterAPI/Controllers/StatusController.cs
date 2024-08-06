using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gateway.AdapterAPI.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Vectorization API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get() => new OkObjectResult(new ServiceStatusInfo
        {
            Name = ServiceNames.GatewayAdapterAPI,
            InstanceName = ValidatedEnvironment.MachineName,
            Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
            Status = ServiceStatuses.Ready
        });

        private static readonly string[] MethodNames = ["GET", "POST", "OPTIONS", "DELETE"];

        /// <summary>
        /// Returns the allowed HTTP methods for the Vectorization API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", MethodNames);

            return Ok();
        }
    }
}
