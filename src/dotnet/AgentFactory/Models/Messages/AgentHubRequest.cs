using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// Represents a request that is sent to an AgentHub to get lists of agents.
    /// </summary>
    public class AgentHubRequest : OrchestrationRequest
    {
        /// <summary>
        /// Conversation history of the current chat session.
        /// </summary>
        [JsonProperty("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; set; }
    }
}
