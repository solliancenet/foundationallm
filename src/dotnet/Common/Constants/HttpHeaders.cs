namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Common HTTP headers used throughout the FoundationaLLM infrastructure.
    /// </summary>
    public static class HttpHeaders
    {
        /// <summary>
        /// API key header used by APIs to authenticate requests.
        /// </summary>
        public const string APIKey = "X-API-KEY";

        /// <summary>
        /// User identity header used by APIs to pass user identity information.
        /// </summary>
        public const string UserIdentity = "X-USER-IDENTITY";

        /// <summary>
        /// Agent access token header used by APIs to implement API key authentication for FoundationaLLM agents.
        /// </summary>
        public const string AgentAccessToken = "X-AGENT-ACCESS-TOKEN";
    }
}
