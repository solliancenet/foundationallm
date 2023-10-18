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
            var dataSourceResponse = await _dataSourceHubService.ResolveRequest(_agentMetadata.AllowedDataSourceNames, userContext);

            MetadataBase dataSourceMetadata = null;

            var dataSource = dataSourceResponse.DataSources[0];

            switch (dataSource.UnderlyingImplementation)
            {
                case "blob-storage":
                    dataSourceMetadata = new BlobStorageDataSource
                    {
                        Name = dataSource.Name,
                        Type = dataSource.UnderlyingImplementation,
                        Description = dataSource.Description,
                        Configuration = new BlobStorageConfiguration
                        {
                            ConnectionStringSecretName = dataSource.Authentication["connection_string_secret"],
                            ContainerName = dataSource.Container,
                            Files = dataSource.Files
                        }
                    };
                    break;
                case "search-service":
                    break;
                case "sql":
                    dataSourceMetadata = new SQLDatabaseDataSource
                    {
                        Name = dataSource.Name,
                        Type = dataSource.UnderlyingImplementation,
                        Description = dataSource.Description,
                        Configuration = new SQLDatabaseConfiguration
                        {
                            Dialect = dataSource.Dialect,
                            Host = dataSource.Authentication["host"],
                            Port = Convert.ToInt32(dataSource.Authentication["port"]),
                            DatabaseName = dataSource.Authentication["database"],
                            Username = dataSource.Authentication["username"],
                            PasswordSecretName = dataSource.Authentication["password_secret"],
                            IncludeTables = dataSource.IncludeTables,
                            FewShotExampleCount = dataSource.FewShotExampleCount ?? 0
                        }
                    };
                    break;
                default:
                    throw new ArgumentException($"The {dataSourceResponse.DataSources[0].UnderlyingImplementation} data source type is not supported.");
            }

            //create LLMOrchestrationCompletionRequest template
            _completionRequestTemplate = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = null, // to be filled in GetCompletion / GetSummary
                Agent = new Agent()
                {
                    Name = _agentMetadata.Name,
                    Type = dataSourceResponse.DataSources[0].UnderlyingImplementation,
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
                DataSourceMetadata = dataSourceMetadata,
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
