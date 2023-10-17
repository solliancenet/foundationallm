using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    public class AgentBase
    {
        protected readonly AgentMetadata _agentMetadata;
        protected readonly ILLMOrchestrationService _orchestrationService;
        protected readonly IPromptHubAPIService _promptHubService;
        protected readonly IDataSourceHubAPIService _dataSourceHubService;

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

        public virtual async Task Configure(string userPrompt, string userContext)
        {
        }

        public virtual async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null;
        }

        public virtual async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            await Task.CompletedTask;
            return null;
        }
    }
}
