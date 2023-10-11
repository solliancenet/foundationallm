using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Implements the <see cref="IConfigurationService"/> interface using Azure Key Vault.
    /// </summary>
    public class KeyVaultConfigurationService : IConfigurationService
    {
        private readonly SecretClient _secretClient;

        /// <summary>
        /// Creates a new instance of the <see cref="KeyVaultConfigurationService"/> class.
        /// </summary>
        /// <param name="settings">The configuration settings for the deployed Key Vault service.</param>
        public KeyVaultConfigurationService(IOptions<KeyVaultConfigurationServiceSettings> settings)
        {
            if (string.IsNullOrEmpty(settings.Value.KeyVaultUri))
                throw new ArgumentNullException(nameof(settings.Value.KeyVaultUri));

#if DEBUG
            // Configure the below options to speed up initial call to Key Vault when debugging locally.
            // You will need to exclude or include the appropriate credential types based on your local environment.
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeEnvironmentCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = false,
                ExcludeAzureCliCredential = true,
                ExcludeManagedIdentityCredential = true
            });
#else
            // This is required to allow the Key Vault service to be accessed from a deployed environment.
            var credential = new DefaultAzureCredential();
#endif

            _secretClient = new SecretClient(
                new Uri(settings.Value.KeyVaultUri),
                credential);
        }

        /// <summary>
        /// Retrieves a secret by the Key Vault secret name.
        /// </summary>
        /// <param name="key">The Key Vault secret name used to retrieve the secret.</param>
        /// <returns>The Key Vault secret.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetValue<T>(string key)
        {
            try
            {
                // Retrieve the secret value
                var secret = _secretClient.GetSecret(key);
                // Cast the secret value to the provided type.
                return (T)Convert.ChangeType(secret.Value.Value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving secret: {key}", ex);
            }
        }
    }
}
