using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// Agent metadata model.
    /// </summary>
    public class Agent: MetadataBase
    {
        /// <summary>
        /// The prompt prefix to assign the agent.
        /// </summary>
        [JsonProperty("prompt_prefix")]
        public string? PromptPrefix { get; set; }

        /// <summary>
        /// The prompt suffix to assign the agent.
        /// </summary>
        [JsonProperty("prompt_suffix")]
        public string? PromptSuffix { get; set; }
    }
}
