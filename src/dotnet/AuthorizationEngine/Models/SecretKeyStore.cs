using FoundationaLLM.Common.Models.Security;
using System.Text.Json.Serialization;

namespace FoundationaLLM.AuthorizationEngine.Models
{
    /// <summary>
    /// Models the content of the secret keys store managed by the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public class SecretKeyStore
    {
        /// <summary>
        /// The unique identifier of the FoundationaLLM instance.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public required string InstanceId { get; set; }

        /// <summary>
        /// The lists of all secret keys in the FoundationaLLM instance grouped by the context identifier.
        /// </summary>
        /// <remarks>
        /// The keys of the dictionary are the secret keys context identifiers.
        /// </remarks>
        [JsonPropertyName("secret_keys")]
        public required Dictionary<string, List<PersistedSecretKey>> SecretKeys { get; set; } = [];
    }
}
