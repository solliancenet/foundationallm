using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    /// <summary>
    /// Contains the agent definitions for use in the FoundationaLLM Core examples.
    /// These definitions are used to create the agents in the FoundationaLLM Core examples.
    /// </summary>
    public static class AgentCatalog
    {
        #region Knowledge Management agents
        /// <summary>
        /// Catalog of knowledge management agents.
        /// </summary>
        public static readonly List<KnowledgeManagementAgent> KnowledgeManagementAgents =
        [
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.GenericInlineContextAgentName,
                Description = "A generic agent that can handle inline context completions.",
                InlineContext = true,
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectId = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistory = new ConversationHistory
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                Gatekeeper = new Gatekeeper
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new OrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain,
                    EndpointConfiguration = new Dictionary<string, object>
                    {
                        { "auth_type", "key" },
                        { "provider", "microsoft" },
                        { "endpoint", "FoundationaLLM:AzureOpenAI:API:Endpoint" },
                        { "api_key", "FoundationaLLM:AzureOpenAI:API:Key" },
                        { "api_version", "FoundationaLLM:AzureOpenAI:API:Version" }
                    },
                    ModelParameters = new Dictionary<string, object>
                    {
                        { "temperature", 0 },
                        { "deployment_name", "completions" }
                    }
                }
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.AzureOpenAIDirectInlineContextAgentName,
                Description = "An AzureOpenAIDirect Agent for generic completions.",
                InlineContext = true,
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectId = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistory = new ConversationHistory
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                Gatekeeper = new Gatekeeper
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new OrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.AzureOpenAIDirect,
                    EndpointConfiguration = new Dictionary<string, object>
                    {
                        { "auth_type", "key" },
                        { "provider", "microsoft" },
                        { "endpoint", "FoundationaLLM:AzureOpenAI:API:Endpoint" },
                        { "api_key", "FoundationaLLM:AzureOpenAI:API:Key" },
                        { "api_version", "FoundationaLLM:AzureOpenAI:API:Version" },
                        { "operation_type", "chat" }
                    },
                    ModelParameters = new Dictionary<string, object>
                    {
                        { "temperature", 0 },
                        { "deployment_name", "completions" }
                    }
                }
            }
        ];
        #endregion

        /// <summary>
        /// Retrieves all agents defined in the catalog.
        /// </summary>
        /// <returns></returns>
        public static List<AgentBase> GetAllAgents()
        {
            var agents = new List<AgentBase>();
            agents.AddRange(KnowledgeManagementAgents);
            
            return agents;
        }
    }
}
