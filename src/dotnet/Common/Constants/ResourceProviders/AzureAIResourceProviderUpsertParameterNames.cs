namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants of the names of the parameters used in the FoundationaLLM.AzureAI resource provider upsert operations.
    /// </summary>
    public class AzureAIResourceProviderUpsertParameterNames
    {
        /// <summary>
        /// The FoundationaLLM object identifier of the agent.
        /// </summary>
        public const string AgentObjectId = "agent-object-id";

        /// <summary>
        /// The FoundationaLLM conversation identifier.
        /// </summary>
        public const string ConversationId = "conversation-id";

        /// <summary>
        /// The identifier of the Azure AI Agent Service agent.
        /// </summary>
        public const string AzureAIAgentId = "azureai-agent-id";

        /// <summary>
        /// Indicates whether the agent thread associated with the <see cref="ConversationId"/> must be created.
        /// </summary>
        public const string MustCreateAzureAIAgentThread = "must-create-azureai-agent-thread";

        /// <summary>
        /// The FoundationaLLM identifier of the attachment.
        /// </summary>
        public const string AttachmentObjectId = "attachment-object-id";

        /// <summary>
        /// Indicates whether the attachment identified by <see cref="AttachmentObjectId"/> must be added to the Azure AI Agent Service file store.
        /// </summary>
        public const string MustCreateAzureAIAgentFile = "must-create-azureai-agent-file";
    }
}
