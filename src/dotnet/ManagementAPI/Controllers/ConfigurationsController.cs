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
    /// Provides methods for interacting with the Configuration Management service.
    /// </summary>
    /// <remarks>
    /// Constructor for the Configurations Controller.
    /// </remarks>
    /// <param name="configurationManagementService">The Configuration Management service
    /// provides methods for managing configurations for FoundationaLLM.</param>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/providersX/{ResourceProviderNames.FoundationaLLM_Configuration}/configurations")]
    public class ConfigurationsController(
        IConfigurationManagementService configurationManagementService) : ControllerBase
    {
        /// <summary>
        /// Returns the branding configuration from app configuration.
        /// </summary>
        [HttpGet("branding", Name = "GetBrandingConfigurations")]
        public async Task<ClientBrandingConfiguration> GetBrandingConfigurations() =>
            await configurationManagementService.GetBrandingConfigurationAsync();

        /// <summary>
        /// Updates the branding configuration in app configuration.
        /// </summary>
        /// <param name="brandingConfiguration"></param>
        /// <returns></returns>
        [HttpPut("branding", Name = "UpdateBrandingConfigurations")]
        public async Task UpdateBrandingConfigurations([FromBody] ClientBrandingConfiguration brandingConfiguration) =>
            await configurationManagementService.SetBrandingConfiguration(brandingConfiguration);

        /// <summary>
        /// Returns the configuration for global agent hints and feature setting.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agentHints", Name = "GetAgentHints")]
        public async Task<AgentHints> GetAgentHints()
        {
            var agentHintsEnabled = await configurationManagementService.GetAllowAgentSelectionAsync();
            return new AgentHints
            {
                Enabled = agentHintsEnabled
            };
        }

        /// <summary>
        /// Updates the configuration for global agent hints and feature setting.
        /// </summary>
        /// <param name="agentHints"></param>
        /// <returns></returns>
        [HttpPut("agentHints", Name = "UpdateAgentHints")]
        public async Task UpdateAgentHints([FromBody] AgentHints agentHints) =>
            await configurationManagementService.SetAllowAgentSelectionAsync(agentHints.Enabled);
    }
}
