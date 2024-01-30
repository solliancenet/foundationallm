using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Agent.Models.Metadata
{
    /// <summary>
    /// The Knowledge Management agent metadata model.
    /// </summary>
    public class KnowledgeManagementAgent : AgentBase
    {
        /// <summary>
        /// The vectorization indexing profile resource path.
        /// </summary>
        [JsonProperty("indexing_profile")]
        public string? IndexingProfile { get; set; }
        /// <summary>
        /// The vectorization embedding profile resource path.
        /// </summary>
        [JsonProperty("embedding_profile")]
        public string? EmbeddingProfile { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public KnowledgeManagementAgent() =>
            Type = Common.Constants.AgentTypes.KnowledgeManagement;
    }
}
