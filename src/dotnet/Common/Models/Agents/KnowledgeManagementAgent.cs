using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Agents
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class KnowledgeManagementAgent : AgentBase
    {
        /// <summary>
        /// The vectorization content source profile.
        /// </summary>
        [JsonPropertyName("content_source_profile_object_id")]
        public string? ContentSourceProfileObjectId { get; set; }

        /// <summary>
        /// The vectorization indexing profile resource path.
        /// </summary>
        [JsonPropertyName("indexing_profile_object_ids")]
        public List<string>? IndexingProfileObjectIds { get; set; }

        /// <summary>
        /// The vectorization text embedding profile resource path.
        /// </summary>
        [JsonPropertyName("text_embedding_profile_object_id")]
        public string? TextEmbeddingProfileObjectId { get; set; }

        /// <summary>
        /// The vectorization text partitioning profile resource path. 
        /// </summary>
        [JsonPropertyName("text_partitioning_profile_object_id")]
        public string? TextPartitioningProfileObjectId { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public KnowledgeManagementAgent() =>
            Type = AgentTypes.KnowledgeManagement;
    }
}
