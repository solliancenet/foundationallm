using Asp.Versioning;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Management.Interfaces;
using FoundationaLLM.Management.Models.Configuration.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides methods for implementing the cache provider.
    /// </summary>
    /// <remarks>
    /// Constructor for the Caches Controller.
    /// </remarks>
    /// <param name="cacheManagementService">Provides cache management methods for managing
    /// configuration and state of downstream services.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/providersX/{ResourceProviderNames.FoundationaLLM_Configuration}/caches")]
    public class CachesController(
        ICacheManagementService cacheManagementService) : ControllerBase
    {
        /// <summary>
        /// Clears the agent cache from the relevant downstream services.
        /// </summary>
        /// <returns></returns>
        [HttpPost("agent/clear", Name = "ClearAgentCache")]
        public async Task<APICacheRefreshResult> ClearAgentCache()
        {
            var result = await cacheManagementService.ClearAgentCache();
            return new APICacheRefreshResult
            {
                Success = result,
                Detail = result ? "Successfully cleared agent cache." : "Failed to clear agent cache."
            };
        }

        /// <summary>
        /// Clears the datasource cache from the relevant downstream services.
        /// </summary>
        /// <returns></returns>
        [HttpPost("datasource/clear", Name = "ClearDataSourceCache")]
        public async Task<APICacheRefreshResult> ClearDataSourceCache()
        {
            var result = await cacheManagementService.ClearDataSourceCache();
            return new APICacheRefreshResult
            {
                Success = result,
                Detail = result ? "Successfully cleared datasource cache." : "Failed to clear datasource cache."
            };
        }

        /// <summary>
        /// Clears the prompt cache from the relevant downstream services.
        /// </summary>
        /// <returns></returns>
        [HttpPost("prompt/clear", Name = "ClearPromptCache")]
        public async Task<APICacheRefreshResult> ClearPromptCache()
        {
            var result = await cacheManagementService.ClearPromptCache();
            return new APICacheRefreshResult
            {
                Success = result,
                Detail = result ? "Successfully cleared prompt cache." : "Failed to clear prompt cache."
            };
        }
    }
}
