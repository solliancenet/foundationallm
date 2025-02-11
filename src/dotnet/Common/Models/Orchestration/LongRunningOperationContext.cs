using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Provides a context for long-running operations.
    /// </summary>
    public class LongRunningOperationContext
    {
        /// <summary>
        /// Gets or sets the FoundationaLLM instance identifier.
        /// </summary>
        [JsonProperty("instanceId")]
        public required string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the long-running operation.
        /// </summary>
        [JsonProperty("id")]
        public required string OperationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the agent that runs the long-running operation.
        /// </summary>
        [JsonProperty("agentName")]
        public required string AgentName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the agent that runs the long-running operation.
        /// </summary>
        [JsonProperty("agentDisplayName")]
        public string? AgentDisplayName { get; set; }

        /// <summary>
        /// The API endpoint URL of the main AI model used by the agent workflow.
        /// </summary>
        [JsonProperty("agentWorkflowMainAIModelAPIEndpoint")]
        public string? AgentWorkflowMainAIModelAPIEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the session in which the long-running operation runs.
        /// </summary>
        [JsonProperty("sessionId")]
        public required string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the User Principal Name (UPN) of the user who triggered the long-running operation.
        /// </summary>
        [JsonProperty("upn")]
        public required string UPN { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user message that triggered the long-running operation.
        /// </summary>
        [JsonProperty("userMessageId")]
        public required string UserMessageId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the agent response message sent to the user during the long-running operation.
        /// </summary>
        [JsonProperty("agentMessageId")]
        public required string AgentMessageId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the completion prompt used in the long-running operation.
        /// </summary>
        [JsonProperty("completionPromptId")]
        public required string CompletionPromptId { get; set; }

        /// <summary>
        /// Gets or sets the Gatekeeper override option.
        /// </summary>
        [JsonProperty("gatekeeperOverride")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AgentGatekeeperOverrideOption GatekeeperOverride { get; set; }

        /// <summary>
        /// Gets or sets a list of Gatekeeper feature names used by the orchestration request.
        /// </summary>
        [JsonProperty("gatekeeperOptions")]
        public string[] GatekeeperOptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the name of the orchestrator used in the long-running operation.
        /// </summary>
        [JsonProperty("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Gets or sets the settings for the semantic cache.
        /// </summary>
        [JsonProperty("semanticCacheSettings")]
        public AgentSemanticCacheSettings? SemanticCacheSettings { get; set; }

        /// <summary>
        /// Gets or sets the Time to Live (TTL) of the long-running operation.
        /// </summary>
        [JsonProperty("ttl")]
        public int TTL { get; set; } = 2592000; // 30 days

        /// <summary>
        /// Gets or sets the iteration number of the long-running operation.
        /// </summary>
        [JsonProperty("statusUpdateIteration")]
        public int StatusUpdateIteration { get; set; } = 0;

        /// <summary>
        /// Gets or sets the start time of the long-running operation.
        /// </summary>
        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }
    }
}
