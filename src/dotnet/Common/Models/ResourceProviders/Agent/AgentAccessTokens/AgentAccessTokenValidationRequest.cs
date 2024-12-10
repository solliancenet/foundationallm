using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens
{
    /// <summary>
    /// Agent access token validation request object.
    /// </summary>
    public class AgentAccessTokenValidationRequest
    {
        /// <summary>
        /// The access token to validate.
        /// </summary>
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }
    }
}
