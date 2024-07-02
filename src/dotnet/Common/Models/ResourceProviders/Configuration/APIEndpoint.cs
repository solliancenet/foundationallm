using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Represents an api endpoint resource.
    /// </summary>
    public class APIEndpoint : ResourceBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="APIEndpoint"/>.
        /// </summary>
        public APIEndpoint() =>
            Type = ConfigurationTypes.APIEndpoint;

        /// <summary>
        /// The api endpoint category.
        /// </summary>
        [JsonPropertyName("category")]
        public APIEndpointCategory Category { get; set; }

        /// <summary>
        /// The type of authentication required for accessing the API.
        /// </summary>
        [JsonPropertyName("authentication_type")]
        public required string AuthenticationType { get; set; }

        /// <summary>
        /// The base URL of the API endpoint.
        /// </summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }

        /// <summary>
        /// A list of URL exceptions.
        /// </summary>
        [JsonPropertyName("url_exceptions")]
        public List<UrlException> UrlExceptions { get; set; } = new List<UrlException>();

        /// <summary>
        /// The API key used for authentication.
        /// </summary>
        [JsonPropertyName("api_key")]
        public string APIKey { get; set; }

        /// <summary>
        /// The api key configuration name.
        /// </summary>
        [JsonPropertyName("api_key_configuration_name")]
        public string APIKeyConfigurationName { get; set; }

        /// <summary>
        /// The timeout duration in seconds for API calls.
        /// </summary>
        [JsonPropertyName("timeout_seconds")]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// The name of the retry strategy.
        /// </summary>
        [JsonPropertyName("retry_strategy_name")]
        public required string RetryStrategyName { get; set; }
    }

    /// <summary>
    /// Represents an exception to the base URL.
    /// </summary>
    public class UrlException
    {
        /// <summary>
        /// The user principal name.
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// The alternative URL.
        /// </summary>
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
