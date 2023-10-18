using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    public class DefaultAgent : AgentBase
    {
        LLMOrchestrationCompletionRequest _completionRequestTemplate;

        public DefaultAgent(
            AgentMetadata agentMetadata,
            ILLMOrchestrationService orchestrationService,
            IPromptHubAPIService promptHubService,
            IDataSourceHubAPIService dataSourceHubService)
            : base(agentMetadata, orchestrationService, promptHubService, dataSourceHubService)
        {
        }

        public async override Task Configure(string userPrompt, string userContext)
        {
            //get prompts for the agent from the prompt hub
            var promptResponse = await _promptHubService.ResolveRequest(_agentMetadata.Name, userContext);

            //get data sources listed for the agent           
            var datasourceResponse = await _dataSourceHubService.ResolveRequest(_agentMetadata.AllowedDataSourceNames, userContext);

            //construct the configuration
            var dataSourceConfig = new SQLDatabaseConfiguration()
            {
                Dialect = datasourceResponse.DataSources[0].Dialect,
                Host = datasourceResponse.DataSources[0].Authentication["host"],
                Port = Convert.ToInt32(datasourceResponse.DataSources[0].Authentication["port"]),
                DatabaseName = datasourceResponse.DataSources[0].Authentication["database"],
                Username = datasourceResponse.DataSources[0].Authentication["username"],
                PasswordSecretName = datasourceResponse.DataSources[0].Authentication["password_secret"],
                IncludeTables = datasourceResponse.DataSources[0].IncludeTables,
                FewShotExampleCount = datasourceResponse.DataSources[0].FewShotExampleCount ?? 0
            };

            //create LLMOrchestrationCompletionRequest template
            _completionRequestTemplate = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = null, // to be filled in GetCompletion / GetSummary
                Agent = new Agent()
                {
                    Name = _agentMetadata.Name,
                    Type = datasourceResponse.DataSources[0].UnderlyingImplementation,
                    Description = _agentMetadata.Description,
                    PromptTemplate = promptResponse.Prompts[0].Prompt
                },
                LanguageModel = new LanguageModel()
                {
                    Type = _agentMetadata.LanguageModel.ModelType,
                    Provider = _agentMetadata.LanguageModel.Provider,
                    Temperature = _agentMetadata.LanguageModel.Temperature ?? 0f,
                    UseChat = _agentMetadata.LanguageModel.UseChat ?? true
                },
                DataSourceMetadata = new SQLDatabaseDataSource()
                {
                    Name = datasourceResponse.DataSources[0].Name,
                    Type = datasourceResponse.DataSources[0].UnderlyingImplementation,
                    Description = datasourceResponse.DataSources[0].Description,
                    Configuration = dataSourceConfig
                },
                MessageHistory = null // to be filled in GetCompletion
            };
        }

        public async override Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            _completionRequestTemplate.UserPrompt = completionRequest.UserPrompt;
            _completionRequestTemplate.MessageHistory = completionRequest.MessageHistory;

            var result = await _orchestrationService.GetCompletion(_completionRequestTemplate);

            return new CompletionResponse()
            {
                Completion = result.Completion,
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }

        public async override Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            var summary = await _orchestrationService.GetSummary(summaryRequest.UserPrompt);

            return new SummaryResponse
            {
                Summary = summary
            };
        }
    }
}
