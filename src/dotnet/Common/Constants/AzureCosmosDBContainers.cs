namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Core FoundationaLLM Cosmos DB container names.
    /// </summary>
    public static class AzureCosmosDBContainers
    {
        /// <summary>
        /// Stores chat sessions and related messages.
        /// </summary>
        public const string Sessions = "Sessions";

        /// <summary>
        /// Stores a mapping between user identities and chat sessions.
        /// </summary>
        public const string UserSessions = "UserSessions";

        /// <summary>
        /// Stores user profile data.
        /// </summary>
        public const string UserProfiles = "UserProfiles";

        /// <summary>
        /// Stores state data for background processing.
        /// </summary>
        public const string State = "State";

        /// <summary>
        /// Stores context for long running operations.
        /// </summary>
        public const string Operations = "Operations";

        /// <summary>
        /// The Cosmos DB change feed leases container.
        /// </summary>
        public const string Leases = "leases";

        /// <summary>
        /// Stores agent resource references.
        /// </summary>
        public const string Agents = "Agents";

        /// <summary>
        /// Stores file attachment references.
        /// </summary>
        public const string Attachments = "Attachments";

        /// <summary>
        /// Stores information about external resources (e.g., Azure OpenAI assistants threads and files).
        /// </summary>
        public const string ExternalResources = "ExternalResources";

        /// <summary>
        /// The vector store for cached completions (used by the semantic cache service).
        /// </summary>
        public const string CompletionsCache = "CompletionsCache";
    }
}
