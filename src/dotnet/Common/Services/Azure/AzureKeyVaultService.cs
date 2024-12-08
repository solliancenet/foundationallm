using Azure;
using Azure.Security.KeyVault.Secrets;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Provides access to and management of Azure Key Vault.
    /// </summary>
    /// <param name="secretClient">The Key Vault <see cref="SecretClient"/>.</param>
    public class AzureKeyVaultService(SecretClient secretClient) : IAzureKeyVaultService
    {
        private readonly SecretClient _secretClient = secretClient;

        /// <inheritdoc/>
        public string KeyVaultUri => _secretClient.VaultUri.ToString().ToLower();

        /// <inheritdoc/>
        public async Task<string?> GetSecretValueAsync(string secretName)
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value?.Value;
        }

        /// <inheritdoc/>
        public string? GetSecretValue(string secretName)
        {
            var secret = _secretClient.GetSecret(secretName);
            return secret.Value?.Value;
        }

        /// <inheritdoc/>
        public async Task SetSecretValueAsync(string secretName, string secretValue) => await _secretClient.SetSecretAsync(secretName, secretValue);

        /// <inheritdoc/>
        public async Task<bool> CheckSecretExistsAsync(string secretName)
        {
            try
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                return secret != null && !string.IsNullOrWhiteSpace(secret.Value.Value);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, bool>> CheckKeyVaultSecretsExistAsync(IEnumerable<string?> secretNames)
        {
            var existenceMap = new Dictionary<string, bool>();

            foreach (var name in secretNames)
            {
                if (name != null)
                {
                    existenceMap[name] = await CheckSecretExistsAsync(name);
                }
            }
            return existenceMap;
        }

        /// <inheritdoc/>
        public async Task RemoveSecretAsync(string secretName)
        {
            var deleteOperation = await _secretClient.StartDeleteSecretAsync(secretName);
            await deleteOperation.WaitForCompletionAsync();
        }
    }
}
