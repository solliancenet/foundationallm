using FoundationaLLM.Common.Models.Conversation;
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
        /// Gets or sets the identifier of the long-running operation.
        /// </summary>
        [JsonProperty("id")]
        public required string OperationId { get; set; }

        /// <summary>
        /// Gets or sets the Time to Live (TTL) of the long-running operation.
        /// </summary>
        [JsonProperty("ttl")]
        public int TTL { get; set; } = 2592000; // 30 days

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
    }
}
