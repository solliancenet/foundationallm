using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Management.Interfaces
{
    /// <summary>
    /// Provides methods for managing app configuration.
    /// </summary>
    public interface IConfigurationManagementService
    {
        /// <summary>
        /// Retrieves the allow agent selection feature flag value from app configuration.
        /// </summary>
        /// <returns></returns>
        Task<bool> GetAllowAgentSelectionAsync();

        /// <summary>
        /// Sets the allow agent selection feature flag value in app configuration.
        /// </summary>
        /// <param name="allowAgentSelection">Indicates whether to enable or disable the feature flag.</param>
        /// <returns></returns>
        Task SetAllowAgentSelectionAsync(bool allowAgentSelection);

        /// <summary>
        /// Retrieves the branding configuration from app configuration.
        /// </summary>
        /// <returns></returns>
        Task<ClientBrandingConfiguration> GetBrandingConfigurationAsync();

        /// <summary>
        /// Sets the branding configuration in app configuration.
        /// </summary>
        /// <param name="brandingConfiguration">The branding configuration settings to apply.</param>
        /// <returns></returns>
        Task SetBrandingConfiguration(ClientBrandingConfiguration brandingConfiguration);
    }
}
