using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.ResourceProvider;
using Newtonsoft.Json;

namespace FoundationaLLM.Agent.Models.Metadata
{
    /// <summary>
    /// Base agent metadata model.
    /// </summary>
    public class AgentBase : ResourceBase
    {
        /// <summary>
        /// The agent's language model configuration.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModel? LanguageModel { get; set; }
        /// <summary>
        /// Indicates whether sessions are enabled for the agent.
        /// </summary>
        [JsonProperty("sessions_enabled")]
        public bool SessionsEnabled { get; set; }
        /// <summary>
        /// The agent's conversation history configuration.
        /// </summary>
        [JsonProperty("conversation_history")]
        public ConversationHistory? ConversationHistory { get; set; }
        /// <summary>
        /// The agent's Gatekeeper configuration.
        /// </summary>
        [JsonProperty("gatekeeper")]
        public Gatekeeper? Gatekeeper { get; set; }
        /// <summary>
        /// The agent's LLM orchestrator type.
        /// </summary>
        [JsonProperty("orchestrator")]
        public string? Orchestrator { get; set; }
        /// <summary>
        /// The agent's prompt.
        /// </summary>
        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }

    /// <summary>
    /// Agent conversation history settings.
    /// </summary>
    public class ConversationHistory
    {
        /// <summary>
        /// Indicates whether the conversation history is enabled.
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        /// <summary>
        /// The maximum number of turns to store in the conversation history.
        /// </summary>
        [JsonProperty("max_history")]
        public int MaxHistory { get; set; }
    }

    /// <summary>
    /// Agent Gatekeeper settings.
    /// </summary>
    public class Gatekeeper
    {
        /// <summary>
        /// Indicates whether to abide by or override the system settings for the Gatekeeper.
        /// </summary>
        [JsonProperty("use_system_setting")]
        public bool UseSystemSetting { get; set; }
        /// <summary>
        /// If <see cref="UseSystemSetting"/> is false, provides Gatekeeper feature selection.
        /// </summary>
        [JsonProperty("options")]
        public string[]? Options { get; set; }
    }

}
