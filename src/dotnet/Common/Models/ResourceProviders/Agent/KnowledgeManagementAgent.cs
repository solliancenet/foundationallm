using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class KnowledgeManagementAgent : AgentBase
    {
        /// <summary>
        /// Gets or sets the vectorization settings for the agent.
        /// </summary>
        [Obsolete("Use KnowledgeSearch property instead. This property will be removed in future versions.")]
        [JsonPropertyName("vectorization")]
        public AgentVectorizationSettings? Vectorization { get; set; }

        /// <summary>
        /// Whether the agent has an inline context and does not rely on an external resource.
        /// </summary>
        [JsonPropertyName("inline_context")]
        public bool InlineContext { get; set; } = false;

        /// <summary>
        /// Set default property values.
        /// </summary>
        public KnowledgeManagementAgent() =>
            Type = AgentTypes.KnowledgeManagement;
    }
}
