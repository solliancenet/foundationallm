using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens
{
    /// <summary>
    /// Agent access token validation result object.
    /// </summary>
    public class AgentAccessTokenValidationResult
    {
        /// <summary>
        /// Gets or sets the flag indicating whether the agent access token is valid.
        /// </summary>
        [JsonPropertyName("valid")]
        public required bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnifiedUserIdentity"/> virtual identity associated with the agent access token.
        /// </summary>
        [JsonPropertyName("virtual_identity")]
        public UnifiedUserIdentity? VirtualIdentity { get; set; }
    }
}
