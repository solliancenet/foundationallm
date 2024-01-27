using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Messages
{
    /// <summary>
    /// The response returned from the Agent Hub.
    /// </summary>
    public record AgentHubResponse
    {
        /// <summary>
        /// Information about a requested agent from the Agent Hub.
        /// </summary>
        //[JsonObject]
        [JsonProperty("agent")]
        public AgentMetadata? Agent { get; set; }

    }

    /// <summary>
    /// The information about an agent returned from the Agent Hub.
    /// </summary>
    public class AgentMetadata : MetadataBase
    {
        /// <summary>
        /// The orchestration engine to use.
        /// </summary>
        [JsonProperty("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Datasources that are used or available to the agent.
        /// </summary>
        [JsonProperty("allowed_data_source_names")]
        public List<string>? AllowedDataSourceNames { get; set; }

        /// <summary>
        /// The lanauge model used by the agent.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModel? LanguageModel { get; set; }

        /// <summary>
        /// The embedding model used by the agent.
        /// </summary>
        [JsonProperty("embedding_model")]
        public EmbeddingModel? EmbeddingModel { get; set; }

        /// <summary>
        /// The maximum number of messages from the chat history to be
        /// taken into consideration when building the prompt.
        /// </summary>
        [JsonProperty("max_message_history_size")]
        public int? MaxMessageHistorySize { get; set; }

        /// <summary>
        /// The prompt container from which prompt values will be retrieved.
        /// </summary>
        [JsonProperty("prompt_container")]
        public string? PromptContainer { get; set; }
    }
}
