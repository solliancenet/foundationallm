using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureAI
{
    /// <summary>
    /// Provides details about a conversation mapping between FoundationaLLM and the Azure AI Agent Service.
    /// </summary>
    public class AzureAIAgentConversationMapping : AzureAIAgentResourceBase
    {
        /// <summary>
        /// The FoundationaLLM conversation (session) id.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        public required string ConversationId { get; set; }

        /// <summary>
        /// The Azure AI Agent Service agent id.
        /// </summary>
        [JsonPropertyName("azureai_agent_id")]
        public required string AzureAIAgentId { get; set; }

        /// <summary>
        /// The Azure AI Agent Service thread id associated with the FoundationaLLM conversation (session) id.
        /// </summary>
        [JsonPropertyName("azure_ai_agent_thread_id")]
        public string? AzureAIAgentThreadId { get; set; }

        /// <summary>
        /// The time at which the Azure AI Agent Service thread was created.
        /// </summary>
        [JsonPropertyName("azureai_agent_thread_created_on")]
        public DateTimeOffset? AzureAIAgentThreadCreatedOn { get; set; }

        /// <summary>
        /// The Azure AI Agent service vector store id associated with the FoundationaLLM session (conversation) id.
        /// </summary>
        [JsonPropertyName("azureai_agent_vector_store_id")]
        public string? AzureAIAgentVectorStoreId { get; set; }

        /// <summary>
        /// The time at which the Azure AI Agent Service vector store was created.
        /// </summary>
        [JsonPropertyName("azureai_agent_vector_store_created_on")]
        public DateTimeOffset? AzureAIAgentVectorStoreCreatedOn { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public AzureAIAgentConversationMapping() =>
            Type = AzureAITypes.AgentConversationMapping;
    }
}
