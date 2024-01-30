using FoundationaLLM.Agent.Models.Metadata;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using Newtonsoft.Json;

namespace FoundationaLLM.Agent.Models.Resources
{
    /// <summary>
    /// Provides details about an agent.
    /// </summary>
    public class AgentReference
    {
        /// <summary>
        /// The name of the agent.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// The filename of the agent.
        /// </summary>
        public required string Filename { get; set; }
        /// <summary>
        /// The type of the agent.
        /// </summary>
        public required string Type { get; set; }

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
}
