using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration;

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
        /// <param name="userContext"></param>
        /// <param name="agentHubAPIService"></param>
        /// <param name="orchestrationServices"></param>
        /// <param name="promptHubAPIService"></param>
        /// <param name="dataSourceHubAPIService"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<AgentBase> Build(
            string userPrompt,
            string userContext,
            IAgentHubAPIService agentHubAPIService,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            IPromptHubAPIService promptHubAPIService,
            IDataSourceHubAPIService dataSourceHubAPIService)
        {
            var agentResponse = await agentHubAPIService.ResolveRequest(userPrompt, userContext);
            var agentInfo = agentResponse.Agent;

            // TODO: Extend the Agent Hub API service response to include the orchestrator
            var orchestrationType = string.IsNullOrWhiteSpace(agentResponse.Agent!.Orchestrator) 
                ? "LangChain"
                : agentInfo!.Orchestrator;

            var validType = Enum.TryParse<LLMOrchestrationService>(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
            if (!validType)
                throw new ArgumentException($"The agent factory does not support the {orchestrationType} orchestration type.");
            var orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);

            // TODO: Design a smarter way to instantiate an agent instance based on its name
            AgentBase? agent = null;
            switch (agentInfo!.Name!.ToLower())
            {
                case "default":
                    agent = new DefaultAgent(agentInfo, orchestrationService, promptHubAPIService, dataSourceHubAPIService);
                    break;
                default:
                    throw new ArgumentException($"The agent factory does not recognize the agent {agentInfo.Name}.");
            }

            await agent.Configure(userPrompt, userContext);

            return agent;
        }

        /// <summary>
        /// Used to select the orchestration service for the agent.
        /// </summary>
        /// <param name="orchestrationType"></param>
        /// <param name="orchestrationServices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static ILLMOrchestrationService SelectOrchestrationService(
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
