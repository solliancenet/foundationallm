using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Implements the <see cref="IConfiguration"/> interface using Azure Key Vault.
    /// </summary>
    public class KeyVaultConfiguration : IConfiguration
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultConfiguration> _logger;

        /// <summary>
        /// Creates a new instance of the <see cref="KeyVaultConfiguration"/> class.
        /// </summary>
        /// <param name="keyVaultUri">The URI of the deployed Key Vault service.</param>
        public KeyVaultConfiguration(string keyVaultUri,
            ILogger<KeyVaultConfiguration> logger)
        {
            _logger = logger;
            _secretClient = new SecretClient(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }

        /// <summary>
        /// Retrieves a secret by the Key Vault secret name.
        /// </summary>
        /// <param name="key">The Key Vault secret name used to retrieve the secret.</param>
        /// <returns>The Key Vault secret.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GetValue(string key)
        {
            try
            {
                // Retrieve the secret value
                var secret = _secretClient.GetSecret(key);
                return secret.Value.Value;
            }
            catch (Exception ex)
            {
                var description = $"Error retrieving secret: {key}";
                _logger.LogError(ex, description);
                // Handle exceptions accordingly
                throw new InvalidOperationException(description, ex);
            }
        }
    }
}
