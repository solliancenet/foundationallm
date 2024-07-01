using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provider methods for managing configuration resources.
    /// </summary>
    public interface IConfigurationManagementClient
    {
        /// <summary>
        /// Retrieves all app configuration values.
        /// </summary>
        /// <returns></returns>
        Task<List<ResourceProviderGetResult<AppConfigurationKeyBase>>> GetAppConfigurationsAsync();

        /// <summary>
        /// Retrieves app configuration values by a filter. Either enter an exact key or a
        /// partial key with a wildcard (asterisk: *) to filter results. For example,
        /// "FoundationaLLM:Branding:*" will return all app configuration values that start
        /// with "FoundationaLLM:Branding:".
        /// </summary>
        /// <param name="key">The full key value or key filter with a wildcard.</param>
        /// <returns></returns>
        Task<List<ResourceProviderGetResult<AppConfigurationKeyBase>>> GetAppConfigurationsByFilterAsync(string key);

        /// <summary>
        /// Retrieves all external orchestration services.
        /// </summary>
        /// <returns></returns>
        Task<List<ResourceProviderGetResult<ExternalOrchestrationService>>> GetExternalOrchestrationServicesAsync();

        /// <summary>
        /// Returns a specific external orchestration service by name.
        /// </summary>
        /// <param name="externalOrchestrationServiceName">The name of the external orchestration
        /// service to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<ExternalOrchestrationService>> GetExternalOrchestrationServiceAsync(string externalOrchestrationServiceName);

        /// <summary>
        /// Upserts an app configuration value. If the value does not exist, it will be created.
        /// To create a standard Key-value App Configuration, use the <see cref="AppConfigurationKeyValue"/>
        /// class for the <see cref="appConfiguration"/> parameter. To create a Key Vault reference App Configuration,
        /// use the <see cref="AppConfigurationKeyVaultReference"/> class for the <see cref="appConfiguration"/> parameter.
        /// </summary>
        /// <param name="appConfiguration">Use either the <see cref="AppConfigurationKeyValue"/> type or the
        /// <see cref="AppConfigurationKeyVaultReference"/> type to save the App Config value as a key-value
        /// setting or a Key Vault reference, respectively.</param>
        /// <returns></returns>
        Task<ResourceProviderUpsertResult> UpsertAppConfigurationAsync(AppConfigurationKeyBase appConfiguration);
    }
}
