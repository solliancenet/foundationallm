using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a secret key.
    /// </summary>
    public class SecretKey
    {
        /// <summary>
        /// Gets or sets the unique identifier of the secret key.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the FoundationaLLM instance identifier.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public required string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the context identifier of the secret key.
        /// </summary>
        [JsonPropertyName("context_id")]
        public required string ContextId { get; set; }

        /// <summary>
        /// Gets or sets the description of the secret key.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the secret key is active or not.
        /// </summary>
        [JsonPropertyName("active")]
        public required bool Active { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the secret key.
        /// </summary>
        [JsonPropertyName("expiration_date")]
        public required DateTimeOffset ExpirationDate { get; set; }
    }
}
