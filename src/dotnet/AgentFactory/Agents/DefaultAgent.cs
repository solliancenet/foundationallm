using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Messages;
using FoundationaLLM.Common.Models.Metadata;
using Microsoft.Extensions.Logging;
using Agent = FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata.Agent;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    /// <summary>
    /// DefaultAgent class
    /// </summary>
    public class DefaultAgent : AgentBase
    {
        private LLMOrchestrationCompletionRequest _completionRequestTemplate = null!;
        private readonly ICacheService _cacheService;
        private readonly ICallContext _callContext;
        private readonly ILogger<DefaultAgent> _logger;

        /// <summary>
        /// Constructor for default agent.
        /// </summary>
        /// <param name="agentMetadata"></param>
        /// <param name="cacheService">The <see cref="ICacheService"/> used to cache agent-related artifacts.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="orchestrationService"></param>
        /// <param name="promptHubService"></param>
        /// <param name="dataSourceHubService"></param>
        /// <param name="logger">The logger used for logging.</param>
        public DefaultAgent(
            AgentMetadata agentMetadata,
            ICacheService cacheService,
            ICallContext callContext,
            ILLMOrchestrationService orchestrationService,
            IPromptHubAPIService promptHubService,
            IDataSourceHubAPIService dataSourceHubService,
            ILogger<DefaultAgent> logger)
            : base(agentMetadata, orchestrationService, promptHubService, dataSourceHubService)
        {
            _cacheService = cacheService;
            _callContext = callContext;
            _logger = logger;
        }

        /// <summary>
        /// Used to configure the DeafultAgent class.
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public override async Task Configure(string userPrompt, string sessionId)
        {
            // Get prompts for the agent from the prompt hub.
            var promptResponse = _callContext.AgentHint != null
                ? await _cacheService.Get<PromptHubResponse>(
                    new CacheKey(_callContext.AgentHint.Name!, CacheCategories.Prompt),
                    async () => {
                        return await _promptHubService.ResolveRequest(
                            _agentMetadata.PromptContainer ?? _agentMetadata.Name!,
                            sessionId
                        );
                    },
                    false,
                    TimeSpan.FromHours(1))
                : await _promptHubService.ResolveRequest(
                    _agentMetadata.PromptContainer ?? _agentMetadata.Name!,
                    sessionId
                  );

            if (promptResponse is {Prompt: not null})
            {
                _logger.LogInformation("The DefaultAgent received the following prompt from the Prompt Hub: {PromptName}.",
                    promptResponse!.Prompt!.Name);
            }

            // Get data sources listed for the agent.
            var dataSourceResponse = _callContext.AgentHint != null
                ? await _cacheService.Get<DataSourceHubResponse>(
                    new CacheKey(_callContext.AgentHint.Name!, CacheCategories.DataSource),
                    async () => { return await _dataSourceHubService.ResolveRequest(_agentMetadata.AllowedDataSourceNames!, sessionId); },
                    false,
                    TimeSpan.FromHours(1))
                : await _dataSourceHubService.ResolveRequest(_agentMetadata.AllowedDataSourceNames!, sessionId);

            if (dataSourceResponse is {DataSources: not null})
            {
                _logger.LogInformation(
                    "The DefaultAgent received the following data sources from the Data Source Hub: {DataSourceList}.",
                    string.Join(",", dataSourceResponse!.DataSources!.Select(ds => ds.Name)));
            }

            var dataSourceMetadata = new List<MetadataBase>();

            var dataSources = dataSourceResponse!.DataSources!;
                        
            foreach (var dataSource in dataSources)
            {
                switch (dataSource.UnderlyingImplementation)
                {
                    case "csv":
                    case "generic-resolver":
                    case "blob-storage":
                        dataSourceMetadata.Add(new BlobStorageDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new BlobStorageConfiguration
                            {
                                ConnectionStringSecretName = dataSource.Authentication!["connection_string_secret"],
                                ContainerName = dataSource.Container,
                                Files = dataSource.Files
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;

                    case "search-service":
                        dataSourceMetadata.Add(new SearchServiceDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new SearchServiceConfiguration
                            {
                                Endpoint = dataSource.Authentication!["endpoint"],
                                KeySecret = dataSource.Authentication["key_secret"],
                                IndexName = dataSource.IndexName,
                                EmbeddingFieldName = dataSource.EmbeddingFieldName,
                                TextFieldName = dataSource.TextFieldName,
                                TopN = dataSource.TopN
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;
                    case "anomaly":
                    case "sql":
                        dataSourceMetadata.Add(new SQLDatabaseDataSource
                        {
                            Name = dataSource.Name,
                            Type = dataSource.UnderlyingImplementation,
                            Description = dataSource.Description,
                            Configuration = new SQLDatabaseConfiguration
                            {
                                Dialect = dataSource.Dialect,
                                Host = dataSource.Authentication!["host"],
                                Port = Convert.ToInt32(dataSource.Authentication["port"]),
                                DatabaseName = dataSource.Authentication["database"],
                                Username = dataSource.Authentication["username"],
                                PasswordSecretSettingKeyName = dataSource.Authentication["password_secret"],
                                IncludeTables = dataSource.IncludeTables!,
                                ExcludeTables = dataSource.ExcludeTables!,
                                RowLevelSecurityEnabled = dataSource.RowLevelSecurityEnabled ?? false,
                                FewShotExampleCount = dataSource.FewShotExampleCount ?? 0
                            },
                            DataDescription = dataSource.DataDescription
                        });
                        break;
                    case "cxo":
                        dataSourceMetadata.Add(new CXODataSource
                        {
                            Name = dataSource.Name,
                            Type = _agentMetadata.Type,
                            Description = dataSource.Description,
                            DataDescription = dataSource.DataDescription,
                            Configuration = new CXOConfiguration
                            {
                                Endpoint = dataSource.Authentication!["endpoint"],
                                KeySecret = dataSource.Authentication["key_secret"],
                                IndexName = dataSource.IndexName,
                                EmbeddingFieldName = dataSource.EmbeddingFieldName,
                                TextFieldName = dataSource.TextFieldName,
                                TopN = dataSource.TopN,
                                RetrieverMode = dataSource.RetrieverMode,
                                Company = dataSource.Company,
                                Sources = dataSource.Sources
                            }

                        });
                        break;
                    default:
                        throw new ArgumentException($"The {dataSource.UnderlyingImplementation} data source type is not supported.");
                }
            }

            //create LLMOrchestrationCompletionRequest template
            _completionRequestTemplate = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = null, // to be filled in GetCompletion / GetSummary
                Agent = new FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata.Agent
                {
                    Name = _agentMetadata.Name,
                    Type = _agentMetadata.Type,
                    Description = _agentMetadata.Description,
                    PromptPrefix = promptResponse!.Prompt?.PromptPrefix,
                    PromptSuffix = promptResponse!.Prompt?.PromptSuffix
                },
                LanguageModel = _agentMetadata.LanguageModel,
                EmbeddingModel = _agentMetadata.EmbeddingModel,
                DataSourceMetadata = dataSourceMetadata,
                MessageHistory = null // to be filled in GetCompletion
            };
        }

        /// <summary>
        /// Calls the orchestration service for the agent to get a completion.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public override async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            _completionRequestTemplate.SessionId = completionRequest.SessionId;
            _completionRequestTemplate.UserPrompt = completionRequest.UserPrompt;
            _completionRequestTemplate.MessageHistory = completionRequest.MessageHistory;

            var result = await _orchestrationService.GetCompletion(_completionRequestTemplate);

            return new CompletionResponse
            {
                Completion = result.Completion!,
                UserPrompt = completionRequest.UserPrompt!,
                FullPrompt = result.FullPrompt,
                PromptTemplate = result.PromptTemplate,
                AgentName = result.AgentName,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }

        /// <summary>
        /// Calls the orchestration service for the agent to get a summary.
        /// </summary>
        /// <param name="summaryRequest"></param>
        /// <returns></returns>
        public override async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            var orchestrationRequest = new LLMOrchestrationRequest
            {
                SessionId = summaryRequest.SessionId,
                UserPrompt = summaryRequest.UserPrompt
            };
            var summary = await _orchestrationService.GetSummary(orchestrationRequest);

            return new SummaryResponse
            {
                Summary = summary
            };
        }
    }
}
