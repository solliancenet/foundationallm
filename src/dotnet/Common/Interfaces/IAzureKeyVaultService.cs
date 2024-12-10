namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to and management of Azure Key Vault.
    /// </summary>
    public interface IAzureKeyVaultService
    {
        /// <summary>
        /// Gets the URI of the Key Vault.
        /// </summary>
        string KeyVaultUri { get; }

        /// <summary>
        /// Gets the value of a secret from Key Vault.
        /// </summary>
        /// <param name="secretName">The name of the secret whose value you wish to retrieve.</param>
        /// <returns></returns>
        Task<string?> GetSecretValueAsync(string secretName);

        /// <summary>
        /// Gets the value of a secret from Key Vault.
        /// </summary>
        /// <param name="secretName">The name of the secret whose value you wish to retrieve.</param>
        /// <returns></returns>
        string? GetSecretValue(string secretName);

        /// <summary>
        /// Sets the value of a secret in Key Vault.
        /// </summary>
        /// <param name="secretName">The name of the secret whose value you wish to set.</param>
        /// <param name="secretValue">The secret value.</param>
        /// <returns></returns>
        Task SetSecretValueAsync(string secretName, string secretValue);

        /// <summary>
        /// Returns whether a secret exists in Key Vault.
        /// </summary>
        /// <param name="secretName">The secret name to check.</param>
        /// <returns></returns>
        Task<bool> CheckSecretExistsAsync(string secretName);

        /// <summary>
        /// Returns a map of secret names and whether they exist in Key Vault.
        /// </summary>
        /// <param name="secretNames">The list of secret names to check.</param>
        /// <returns></returns>
        Task<Dictionary<string, bool>> CheckKeyVaultSecretsExistAsync(IEnumerable<string?> secretNames);

        /// <summary>
        /// Removes the specified secret from the Key Vault.
        /// </summary>
        /// <param name="secretName">The name of the secret to be removed.</param>
        /// <returns></returns>
        Task RemoveSecretAsync(string secretName);
    }
}
