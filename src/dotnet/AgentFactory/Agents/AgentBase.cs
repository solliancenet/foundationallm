using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    /// <summary>
    /// Base class for an agent.
    /// </summary>
    public class AgentBase
    {
        /// <summary>
        /// The agent metadata.
        /// </summary>
        protected readonly AgentMetadata _agentMetadata;

        /// <summary>
        /// The orchestration service for the agent.
        /// </summary>
        protected readonly ILLMOrchestrationService _orchestrationService;

        /// <summary>
        /// The prompt hub for the agent.
        /// </summary>
        protected readonly IPromptHubAPIService _promptHubService;

        /// <summary>
        /// The data source hub for the agent.
        /// </summary>
        protected readonly IDataSourceHubAPIService _dataSourceHubService;

        /// <summary>
        /// Constructor for the AgentBase class.
        /// </summary>
        /// <param name="agentMetadata"></param>
        /// <param name="orchestrationService"></param>
        /// <param name="promptHubService"></param>
        /// <param name="dataSourceHubService"></param>
        public AgentBase(
            AgentMetadata agentMetadata,
            ILLMOrchestrationService orchestrationService,
            IPromptHubAPIService promptHubService,
            IDataSourceHubAPIService dataSourceHubService)
        {
            _agentMetadata = agentMetadata;
            _orchestrationService = orchestrationService;
            _promptHubService = promptHubService;
            _dataSourceHubService = dataSourceHubService;
        }

        /// <summary>
        /// This will setup the agent based on its metadata.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Configure(string userPrompt, string sessionId) =>
            await Task.CompletedTask;

        /// <summary>
        /// The call to execute a completion after the agent is configured.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public virtual async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null!;
        }

        /// <summary>
        /// The call to get a summary after the agent has been configured.
        /// </summary>
        /// <param name="summaryRequest"></param>
        /// <returns></returns>
        public virtual async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            await Task.CompletedTask;
            return null!;
        }
    }
}
