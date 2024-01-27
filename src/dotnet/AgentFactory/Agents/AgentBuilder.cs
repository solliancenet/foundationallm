using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Models.Messages;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    /// <summary>
    /// Agent Builder class
    /// </summary>
    public class AgentBuilder
    {
        /// <summary>
        /// Used to build an agenet given the inbound parameters.
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="sessionId"></param>
        /// <param name="cacheService">The <see cref="ICacheService"/> used to cache agent-related artifacts.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="agentHubAPIService"></param>
        /// <param name="orchestrationServices"></param>
        /// <param name="promptHubAPIService"></param>
        /// <param name="dataSourceHubAPIService"></param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<AgentBase> Build(
            string userPrompt,
            string sessionId,
            ICacheService cacheService,
            ICallContext callContext,
            IAgentHubAPIService agentHubAPIService,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            IPromptHubAPIService promptHubAPIService,
            IDataSourceHubAPIService dataSourceHubAPIService,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<AgentBuilder>();
            if (callContext.AgentHint == null)
                logger.LogInformation("The AgentBuilder is starting to build an agent without an agent hint.");
            else
                logger.LogInformation("The AgentBuilder is starting to build an agent with the following agent hint: {AgentName},{IsPrivateAgent}.",
                    callContext.AgentHint.Name, callContext.AgentHint.Private);

            var agentResponse = callContext.AgentHint != null
                ? await cacheService.Get<AgentHubResponse>(
                    new CacheKey(callContext.AgentHint.Name!, CacheCategories.Agent),
                    async () => { return await agentHubAPIService.ResolveRequest(userPrompt, sessionId); },
                    false,
                    TimeSpan.FromHours(1))
                : await agentHubAPIService.ResolveRequest(userPrompt, sessionId);

            var agentInfo = agentResponse!.Agent;

            if (agentResponse is {Agent: not null})
            {
                logger.LogInformation("The AgentBuilder received the following agent from the AgentHub: {AgentName}.",
                    agentResponse.Agent!.Name);
            }

            // TODO: Extend the Agent Hub API service response to include the orchestrator
            var orchestrationType = string.IsNullOrWhiteSpace(agentResponse.Agent!.Orchestrator) 
                ? "LangChain"
                : agentInfo!.Orchestrator;

            var validType = Enum.TryParse<LLMOrchestrationService>(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
            if (!validType)
                throw new ArgumentException($"The agent factory does not support the {orchestrationType} orchestration type.");
            var orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);
            
            AgentBase? agent = null;
            agent = new DefaultAgent(
                agentInfo!,
                cacheService, callContext,
                orchestrationService, promptHubAPIService, dataSourceHubAPIService,
                loggerFactory.CreateLogger<DefaultAgent>());           

            await agent.Configure(userPrompt, sessionId);

            return agent;
        }

        /// <summary>
        /// Used to select the orchestration service for the agent.
        /// </summary>
        /// <param name="orchestrationType"></param>
        /// <param name="orchestrationServices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static ILLMOrchestrationService SelectOrchestrationService(
            LLMOrchestrationService orchestrationType,
            IEnumerable<ILLMOrchestrationService> orchestrationServices)
        {
            Type? orchestrationServiceType = null;

            switch (orchestrationType)
            {
                case LLMOrchestrationService.LangChain:
                    orchestrationServiceType = typeof(ILangChainService);
                    break;
                case LLMOrchestrationService.SemanticKernel:
                    orchestrationServiceType = typeof(ISemanticKernelService);
                    break;
                default:
                    throw new ArgumentException($"The orchestration type {orchestrationType} is not supported.");
            }

            var orchestrationService = orchestrationServices.FirstOrDefault(x => orchestrationServiceType.IsAssignableFrom(x.GetType()));
            if (orchestrationService == null)
                throw new ArgumentException($"There is no orchestration service available for orchestration type {orchestrationType}.");

            return orchestrationService;
        }
    }
}
