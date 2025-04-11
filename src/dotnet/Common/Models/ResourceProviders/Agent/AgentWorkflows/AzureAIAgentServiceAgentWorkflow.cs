using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for an Azure AI Agent Service workflow.
    /// </summary>
    public class AzureAIAgentServiceAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.AzureAIAgentService;

        /// <summary>
        /// The Azure AI Agent ID for the agent workflow.
        /// </summary>
        [JsonPropertyName("agent_id")]
        public string? AgentId { get; set; }

        /// <summary>
        /// The vector store ID for the agent.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

        /// <summary>
        /// The Azure AI project connection string for the agent workflow.
        /// </summary>
        [JsonPropertyName("project_connection_string")]
        public required string ProjectConnectionString { get; set; }
    }
}
