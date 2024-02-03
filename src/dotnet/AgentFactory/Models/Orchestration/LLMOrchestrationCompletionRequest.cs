using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    /// <summary>
    /// Orchestration completion request.
    /// Contains the metadata needed by the orchestration services
    /// to build and execute completions.
    /// </summary>
    public class LLMOrchestrationCompletionRequest : LLMOrchestrationRequest
    {
        /// <summary>
        /// Agent metadata
        /// </summary>
        [JsonProperty("agent")]
        public FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata.Agent? Agent { get; set; }

        /// <summary>
        /// Data source metadata
        /// </summary>
        [JsonProperty("data_sources")]
        public List<MetadataBase>? DataSourceMetadata { get; set; }

        /// <summary>
        /// Language model metadata.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModel? LanguageModel { get; set; }

        /// <summary>
        /// Embedding model metadata.
        /// </summary>
        [JsonProperty("embedding_model")]
        public EmbeddingModel? EmbeddingModel { get; set; }

        /// <summary>
        /// Message history list
        /// </summary>
        [JsonProperty("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; set; }
    }
}
