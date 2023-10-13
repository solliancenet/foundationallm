using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.Common.Models.Chat;
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
        public Agent Agent { get; set; }

        /// <summary>
        /// Data source metadata
        /// </summary>
        [JsonProperty("data_source")]
        public dynamic? DataSource
        {
            get
            {
                switch (Agent.Type)
                {
                    case "anomaly": case "sql":
                        return new SQLDatabaseDataSource();
                    case "csv":
                        return new CSVFileDataSource();
                    default:
                        return null;
                }
            }
            set => DataSource = value;
        }

        /// <summary>
        /// Language model metadata.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModel LanguageModel { get; set; }

        /// <summary>
        /// Message history list
        /// </summary>
        [JsonProperty("message_history")]
        public List<MessageHistoryItem> MessageHistory { get; set; }
    }
}
