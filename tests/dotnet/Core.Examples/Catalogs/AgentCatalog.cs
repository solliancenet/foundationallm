using FoundationaLLM.Common.Constants;
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
                Name = TestAgentNames.GenericAgentName,
                Description = "A generic agent that can handle completions.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.SemanticKernelAgentName,
                Description = "SemanticKernel agent that can handle completions.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null,
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.SemanticKernel,
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.LangChainAgentName,
                Description = "LangChain agent that can handle completions.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null,
                    DataSourceObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.SemanticKernelSDZWA,
                Description = "Knowledge Management Agent that queries the San Diego Zoo Wildlife Alliance journals using SemanticKernel.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.SemanticKernel
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.LangChainSDZWA,
                Description = "Knowledge Management Agent that queries the San Diego Zoo Wildlife Alliance journals using LangChain.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.ConversationGeneratorAgent,
                Description = "An agent that creates conversations based on product descriptions.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain
                },
                AIModelObjectId = TestAIModelNames.Completions_GPT4o
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.Dune01,
                Description = "Knowledge Management Agent that queries the Dune books using SemanticKernel.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.SemanticKernel
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.Dune02,
                Description = "Agent that writes poems about Dune suitable for being used in wartime songs.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.SemanticKernel
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.Dune03,
                Description = "Answers questions about Dune by asking for help from other agents.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.SemanticKernel
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },
            new KnowledgeManagementAgent
            {
                Name = TestAgentNames.LangChainDune,
                Description = "Knowledge Management Agent that queries the Dune books using LangChain.",
                SessionsEnabled = true,
                Vectorization = new AgentVectorizationSettings
                {
                    DedicatedPipeline = false,
                    IndexingProfileObjectIds = null,
                    TextEmbeddingProfileObjectId = null
                },
                ConversationHistorySettings = new ConversationHistorySettings
                {
                    Enabled = true,
                    MaxHistory = 10
                },
                GatekeeperSettings = new GatekeeperSettings
                {
                    UseSystemSetting = false
                },
                OrchestrationSettings = new AgentOrchestrationSettings
                {
                    Orchestrator = LLMOrchestrationServiceNames.LangChain
                },
                AIModelObjectId = TestAIModelNames.Completions_Deployed_Default
            },

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
