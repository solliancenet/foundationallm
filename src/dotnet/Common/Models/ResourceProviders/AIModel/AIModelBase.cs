using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    public class AIModelBase : ResourceBase
    {
        public List<AIModelEndpoint> Endpoints { get; set; }
        public List<AIModel> Models { get; set; }

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                AgentTypes.InternalContext => typeof(KnowledgeManagementAgent), // Temporary until InternalContextAgent is completeyl removed.
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };

    }
}
