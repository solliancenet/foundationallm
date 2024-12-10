using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Authorization.API.Controllers
{
    /// <summary>
    /// Provides methods for processing secret keys requests.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process secret keys requests.</param>
    [Authorize(Policy = "RequiredClaims")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/secretkeys")]
    public class SecretKeysController(
        IAuthorizationCore authorizationCore)
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;

        [HttpGet("{contextId}")]
        public IActionResult GetSecretKeys(string instanceId, string contextId) =>
            new OkObjectResult(
                _authorizationCore.GetSecretKeys(instanceId, contextId));

        [HttpPost]
        public async Task<IActionResult> UpsertSecretKey(string instanceId, [FromBody] SecretKey secretKey) =>
            new OkObjectResult(
                await _authorizationCore.UpsertSecretKey(instanceId, secretKey));

        [HttpDelete("{contextId}")]
        public async Task<IActionResult> DeleteSecretKey(string instanceId, string contextId, string secretKeyId)
        {
            await _authorizationCore.DeleteSecretKey(instanceId, contextId, secretKeyId);

            return new OkResult();
        }

        [HttpPost("{contextId}")]
        public async Task<IActionResult> ValidateSecretKey(string instanceId, string contextId, string secretKeyValue) =>
            new OkObjectResult(
                await _authorizationCore.ValidateSecretKey(instanceId, contextId, secretKeyValue));
    }
}
