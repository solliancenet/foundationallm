using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// Represents strongly-typed user identity information, regardless of
    /// the identity provider.
    /// </summary>
    public class UnifiedUserIdentity
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The username of the user used to authenticate.
        /// </summary>
        [JsonPropertyName("user_name")]
        public string? Username { get; set; }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }

        /// <summary>
        /// The User Principal Name (UPN) of the user.
        /// </summary>
        [JsonPropertyName("upn")]
        public string? UPN { get; set; }

        /// <summary>
        /// Indicates whether the identity is associated with an agent access token.
        /// </summary>
        [JsonPropertyName("associated_with_access_token")]
        public bool AssociatedWithAccessToken { get; set; } = false;

        /// <summary>
        /// The list of the identifiers of the groups to which the user belongs.
        /// </summary>
        [JsonPropertyName("group_ids")]
        public List<string> GroupIds { get; set; } = [];
    }
}
