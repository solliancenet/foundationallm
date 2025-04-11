namespace FoundationaLLM.Common.Models.ResourceProviders.AzureAI
{
    /// <summary>
    /// Represents the result of an upsert operation for an <see cref="AzureAIAgentConversationMapping"/> object.
    /// </summary>
    public class AzureAIAgentConversationMappingUpsertResult : ResourceProviderUpsertResult<AzureAIAgentConversationMapping>
    {
        /// <summary>
        /// The identifier of the newly created Azure AI Agent Service thread id (if any).
        /// </summary>
        public string? NewAzureAIAgentThreadId { get; set; }

        /// <summary>
        /// The identifier of the newly created Azure AI Agent Service vector store id (if any).
        /// </summary>
        public string? NewAzureAIAgentVectorStoreId { get; set; }
    }
}
