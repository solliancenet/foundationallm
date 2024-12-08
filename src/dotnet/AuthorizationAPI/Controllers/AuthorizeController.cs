using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Authorization.API.Controllers
{
    /// <summary>
    /// Provides methods for processing authorization requests.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process authorization requests.</param>
    [Authorize(Policy = "RequiredClaims")]
    [ApiController]
    [Route($"instances/{{instanceId}}/authorize")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class AuthorizeController(
        IAuthorizationCore authorizationCore)
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;

        [HttpPost(Name = "ProcessAuthorizationRequest")]
        public IActionResult ProcessAuthorizationRequest(string instanceId, [FromBody] ActionAuthorizationRequest request) =>
            new OkObjectResult(
                _authorizationCore.ProcessAuthorizationRequest(instanceId, request));
    }
}
