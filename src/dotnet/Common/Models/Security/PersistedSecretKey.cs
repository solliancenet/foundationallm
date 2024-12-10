using FoundationaLLM.Common.Models.Authorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Security
{
    /// <summary>
    /// Represents a persisted secret key.
    /// </summary>
    public class PersistedSecretKey : SecretKey
    {
        /// <summary>
        /// Gets or sets the salt used in the computation of the hash.
        /// </summary>
        /// <remarks>
        /// The salt is a random value that is used to protect against dictionary attacks.
        /// It is generated once and stored with the secret key in a Base58 encoded format.
        /// </remarks>
        [JsonIgnore]
        public string? Salt { get; set; }

        /// <summary>
        /// Gets or sets the hash of the secret key.
        /// </summary>
        /// <remarks>
        /// The hash is computed by applying the hashing algorithm to the secret key and the salt.
        /// </remarks>
        [JsonIgnore]
        public string? Hash { get; set; }

        /// <summary>
        /// Gets the name of the key vault secret that stores the salt.
        /// </summary>
        [JsonIgnore]
        public string SaltKeyVaultSecretName =>
            $"foundationallm-secretkey-salt-{InstanceId.ToLower()}-{Id.ToLower()}";

        /// <summary>
        /// Gets the name of the key vault secret that stores the hash.
        /// </summary>
        [JsonIgnore]
        public string HashKeyVaultSecretName =>
            $"foundationallm-secretkey-hash-{InstanceId.ToLower()}-{Id.ToLower()}";
    }
}
