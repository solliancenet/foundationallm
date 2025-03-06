using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for an Azure OpenAI Assistants workflow.
    /// </summary>
    public class AzureOpenAIAssistantsAgentWorkflow: AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.AzureOpenAIAssistants;
                
        /// <summary>
        /// The OpenAI Assistant ID for the agent workflow.
        /// </summary>
        [JsonPropertyName("assistant_id")]
        public required string AssistantId { get; set; }

        /// <summary>
        /// The vector store ID for the assistant.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }
    }
}
