using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Core API service.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        [AllowAnonymous]
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult GetServiceStatus(string instanceId) =>
            new OkObjectResult(new ServiceStatusInfo
            {
                Name = ServiceNames.CoreAPI,
                InstanceId = instanceId,
                InstanceName = ValidatedEnvironment.MachineName,
                Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
                Status = ServiceStatuses.Ready
            });

        /// <summary>
        /// Returns OK if the requester is authenticated and allowed to execute
        /// requests against this service.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        [HttpGet("auth", Name = "GetAuthStatus")]
        public IActionResult GetAuthStatus(string instanceId) =>
            Ok();

        private static readonly string[] AllowedHttpVerbs = ["GET", "POST", "OPTIONS"];

        /// <summary>
        /// Returns the allowed HTTP methods for the Core API service.
        /// </summary>
        [AllowAnonymous]
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", AllowedHttpVerbs);
            
            return Ok();
        }
    }
}
