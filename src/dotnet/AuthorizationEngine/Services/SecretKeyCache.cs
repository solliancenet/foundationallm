using FoundationaLLM.AuthorizationEngine.Models;
using FoundationaLLM.Common.Models.Security;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FoundationaLLM.AuthorizationEngine.Services
{
    /// <summary>
    /// In-memory cache for secret keys.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SecretKeyCache"/> class.
    /// </remarks>
    /// <param name="secretKeyStore">The <see cref="SecretKeyStore"/> containing the data to initialize the cache.</param>
    /// <param name="configuration">The application configuration values.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class SecretKeyCache(
        SecretKeyStore secretKeyStore,
        IConfiguration configuration,
        ILogger logger)
    {
        private readonly Dictionary<string, Dictionary<string, PersistedSecretKey>> _secretKeys =
            secretKeyStore.SecretKeys
                .ToDictionary(
                    sk => sk.Key,
                    sk => sk.Value.ToDictionary(x => x.Id));
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger _logger = logger;
        private readonly string _instanceId = secretKeyStore.InstanceId;

        /// <summary>
        /// Gets the secret key associated with the specified context and key identifier.
        /// </summary>
        /// <param name="contextId">The context to which the secret key belongs.</param>
        /// <param name="keyId">The identifier of the secret key.</param>
        /// <param name="key">The <see cref="PersistedSecretKey"/> that is looked for.</param>
        /// <returns><see langword="true"/> if the secret key was found, <see langword="false"/> otherwise.</returns>
        public bool TryGetKey(string contextId, string keyId, [MaybeNullWhen(false)] out PersistedSecretKey key)
        {
            if (_secretKeys.TryGetValue(contextId, out var persistedKeys))
            {
                if (persistedKeys.TryGetValue(keyId, out var persistedKey))
                {
                    if (EnsureSaltAndHash(persistedKey))
                    {
                        key = persistedKey;
                        return true;
                    }
                    else
                    {
                        _logger.LogError(
                            "Secret key with id {KeyId} in context {ContextId} and instance {InstanceId} is missing salt or hash.",
                            persistedKey.Id,
                            persistedKey.ContextId,
                            persistedKey.InstanceId);
                        key = null;
                        return false;
                    }
                }
                else
                {
                    _logger.LogError(
                    "An invalid secret key id was specified when looking for secret key with id {KeyId} in context {ContextId} and instance {InstanceId}.",
                    keyId,
                    contextId,
                    _instanceId);
                }
            }
            else
            {
                _logger.LogError(
                    "An invalid context was specified when looking for secret key with id {KeyId} in context {ContextId} and instance {InstanceId}.",
                    keyId,
                    contextId,
                    _instanceId);
            }

            key = null;
            return false;
        }

        /// <summary>
        /// Gets the secret key associated with the specified context and key identifier.
        /// </summary>
        /// <param name="contextId">The context to which the secret keys belong.</param>
        /// <returns></returns>
        public List<PersistedSecretKey> GetKeys(string contextId)
        {
            if (_secretKeys.TryGetValue(contextId, out var persistedKeys))
            {
                return [.. persistedKeys.Values];
            }
            else
            {
                _logger.LogError(
                    "An invalid context was specified when looking for secret keys in context {ContextId} and instance {InstanceId}.",
                    contextId,
                    _instanceId);
            }

            return [];
        }


        /// <summary>
        /// Adds or updates a secret key in the cache.
        /// </summary>
        /// <param name="persistedSecretKey">The <see cref="PersistedSecretKey"/> to add or update in the cache.</param>
        public void AddOrUpdatePersistedSecretKey(PersistedSecretKey persistedSecretKey)
        {
            if (!_secretKeys.TryGetValue(persistedSecretKey.ContextId, out var secretKeys))
            {
                secretKeys = [];
                _secretKeys[persistedSecretKey.ContextId] = secretKeys;
            }

            if (!secretKeys.TryGetValue(persistedSecretKey.Id, out var secretKey))
            {
                secretKeys.Add(persistedSecretKey.Id, persistedSecretKey);
            }
            else
            {
                secretKeys[persistedSecretKey.Id] = persistedSecretKey;
            }
        }

        /// <summary>
        /// Adds or updates a secret key in the cache.
        /// </summary>
        /// <param name="persistedSecretKey">The <see cref="PersistedSecretKey"/> to add or update in the cache.</param>
        public void RemovePersistedSecretKey(PersistedSecretKey persistedSecretKey)
        {
            if (_secretKeys.TryGetValue(persistedSecretKey.ContextId, out var secretKeys))
            {
                if (secretKeys.TryGetValue(persistedSecretKey.Id, out var secretKey))
                {
                    secretKeys.Remove(secretKey.Id);
                }
            }
        }

        private bool EnsureSaltAndHash(PersistedSecretKey key)
        {
            key.Salt ??= _configuration[key.SaltKeyVaultSecretName];
            key.Hash ??= _configuration[key.HashKeyVaultSecretName];

            return !string.IsNullOrWhiteSpace(key.Salt) && !string.IsNullOrWhiteSpace(key.Hash);
        }
    }
}
