namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides well-known parameter names for agent tools.
    /// </summary>
    public static class AgentToolPropertyNames
    {
        /// <summary>
        /// Indicates whether the agent tool requires a code session.
        /// </summary>
        public const string CodeSessionRequired = "code_session_required";

        /// <summary>
        /// The name of the code session provider.
        /// </summary>
        public const string CodeSessionEndpointProvider = "code_session_endpoint_provider";

        /// <summary>
        /// The programming language of the code session.
        /// </summary>
        public const string CodeSessionLanguage = "code_session_language";

        /// <summary>
        /// The identifier of the code session.
        /// </summary>
        public const string CodeSessionId = "code_session_id";

        /// <summary>
        /// The code session endpoint.
        /// </summary>
        public const string CodeSessionEndpoint = "code_session_endpoint";
    }
}
