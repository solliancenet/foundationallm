using Azure;
using Azure.Data.AppConfiguration;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Common.Authentication;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Provides access to and management of Azure App Configuration.
    /// </summary>
    /// <param name="configurationClient">The Azure App Configuration <see cref="ConfigurationClient"/>.</param>
    /// <param name="logger">The logger.</param>
    public class AzureAppConfigurationService(
        ConfigurationClient configurationClient,
        ILogger<AzureAppConfigurationService> logger) : IAzureAppConfigurationService
    {
        private readonly ConfigurationClient _configurationClient = configurationClient;
        private readonly ILogger<AzureAppConfigurationService> _logger = logger;

        /// <inheritdoc/>
        public async Task<string?> GetConfigurationSettingAsync(string key)
        {
            var setting = await _configurationClient.GetConfigurationSettingAsync(key, null);
            return setting.Value?.Value;
        }

        /// <inheritdoc/>
        public async Task<List<(string Key, string? Value, string ContentType)>> GetConfigurationSettingsAsync(string keyFilter)
        {
            var settings = _configurationClient.GetConfigurationSettingsAsync(new SettingSelector
            {
                KeyFilter = keyFilter,
                Fields = SettingFields.Key | SettingFields.Value | SettingFields.ContentType
            });

            var settingList = new List<(string Key, string? Value, string ContentType)>();
            await foreach (var setting in settings)
            {
                settingList.Add((setting.Key, setting.Value, setting.ContentType));
            }
            return settingList;
        }

        /// <inheritdoc/>
        public async Task SetConfigurationSettingAsync(string key, string value, string contentType)
        {
            var setting = new ConfigurationSetting(key, value)
            {
                ContentType = contentType
            };
            var response = await _configurationClient.SetConfigurationSettingAsync(setting);

            var rawResponse = response.GetRawResponse();
            if (rawResponse.Status != (int)HttpStatusCode.OK)
                throw new Exception($"Failed to set app configuration setting ({key}).");
        }

        /// <inheritdoc/>
        public async Task<bool> GetFeatureFlagAsync(string key)
        {
            var enabled = false;
            try
            {
                if (!key.StartsWith(FeatureFlagConfigurationSetting.KeyPrefix))
                {
                    key = FeatureFlagConfigurationSetting.KeyPrefix + key;
                }
                var featureFlagSetting = await _configurationClient
                    .GetConfigurationSettingAsync(key);
                if (featureFlagSetting.HasValue && featureFlagSetting.Value is FeatureFlagConfigurationSetting featureFlag)
                {
                    enabled = featureFlag.IsEnabled;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting feature flag ({Key}) from app configuration.", key);
            }

            return enabled;
        }

        /// <inheritdoc/>
        public async Task SetFeatureFlagAsync(string key, bool flagEnabled)
        {
            try
            {
                var featureFlagSetting = new FeatureFlagConfigurationSetting(
                    key, isEnabled: flagEnabled);
                await _configurationClient.SetConfigurationSettingAsync(featureFlagSetting);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting feature flag ({Key}) in app configuration.", key);
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, bool>> CheckAppConfigurationSettingsExistAsync()
        {
            var settings = _configurationClient.GetConfigurationSettingsAsync(new SettingSelector { KeyFilter = "*" });

            var existenceMap = new Dictionary<string, bool>();
            await foreach (var setting in settings)
            {
                existenceMap[setting.Key] = !string.IsNullOrWhiteSpace(setting.Value);
            }
            return existenceMap;
        }

        /// <inheritdoc/>
        public async Task<bool> CheckAppConfigurationSettingExistsAsync(string key)
        {
            try
            {
                await _configurationClient.GetConfigurationSettingAsync(key);
            }
            catch (RequestFailedException ex)
            {
                if (ex.GetRawResponse()!.Status == 404)
                    return false;
                throw;
            }
            return true;
        }

        /// <inheritdoc/>
        public async Task DeleteAppConfigurationSettingAsync(string key)
        {
            ConfigurationSetting setting = await _configurationClient.GetConfigurationSettingAsync(key);

            if (setting is SecretReferenceConfigurationSetting secretReference)
            {
                var identifier = new KeyVaultSecretIdentifier(secretReference.SecretId);
                _logger.LogInformation("App Configuration key {key} references Key Vault secret {identifier.Name}. Deleting.", key, identifier.Name);
                var secretClient = new SecretClient(identifier.VaultUri, DefaultAuthentication.AzureCredential);
                try
                {
                    await secretClient.StartDeleteSecretAsync(identifier.Name);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error deleting Key Vault secret {key}.", identifier.Name);
                }
            }

            var deleteSettingResponse = await _configurationClient.DeleteConfigurationSettingAsync(key);
            if (deleteSettingResponse.IsError)
                throw new Exception($"Failed to delete App Configuration setting {key}: {deleteSettingResponse.ReasonPhrase}");
        }
    }
}
