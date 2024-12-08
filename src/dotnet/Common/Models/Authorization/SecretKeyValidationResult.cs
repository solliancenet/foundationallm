using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Provides the result of a secret key validation.
    /// </summary>
    public class SecretKeyValidationResult
    {
        /// <summary>
        /// Gets or sets the flag indicating whether the secret key is valid.
        /// </summary>
        [JsonPropertyName("valid")]
        public required bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnifiedUserIdentity"/> virtual identity associated with the secret key.
        /// </summary>
        [JsonPropertyName("virtual_identity")]
        public UnifiedUserIdentity? VirtualIdentity { get; set; }
    }
}
