using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class KnowledgeManagementAgent : AgentBase
    {
        /// <summary>
        /// The vectorization settings for the agent.
        /// </summary>
        [JsonPropertyName("vectorization")]
        public AgentVectorizationSettings Vectorization { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public KnowledgeManagementAgent() =>
            Type = AgentTypes.KnowledgeManagement;
    }
}
