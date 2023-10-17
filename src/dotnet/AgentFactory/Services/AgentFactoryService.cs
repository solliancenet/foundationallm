using FoundationaLLM.AgentFactory.Core.Agents;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.SemanticFunctions;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// AgentFactoryService class.
/// </summary>
public class AgentFactoryService : IAgentFactoryService
{
    private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices;
    private readonly IAgentHubAPIService _agentHubAPIService;
    private readonly AgentFactorySettings _agentFactorySettings;
    private readonly AgentHubSettings _agentHubSettings;
    
    private readonly PromptHubSettings _promptHubSettings;
    private readonly IPromptHubAPIService _promptHubAPIService;

    private readonly DataSourceHubSettings _dataSourceHubSettings;
    private readonly IDataSourceHubAPIService _dataSourceHubAPIService;

    private readonly ILogger<AgentFactoryService> _logger;
    private readonly IUserIdentityContext _userIdentity;

    private LLMOrchestrationService _llmOrchestrationService = LLMOrchestrationService.LangChain;

    /// <summary>
    /// Constructor for the Agent Factory Service
    /// </summary>
    /// <param name="semanticKernel"></param>
    /// <param name="langChain"></param>
    /// <param name="agentHubService"></param>
    /// <param name="agentFactorySettings"></param>
    /// <param name="agentHubSettings"></param>
    /// <param name="promptHubService"></param>
    /// <param name="promptHubSettings"></param>
    /// <param name="dataSourceHubService"></param>
    /// <param name="dataSourceHubSettings"></param>
    /// <param name="logger"></param>
    public AgentFactoryService(
        IEnumerable<ILLMOrchestrationService> orchestrationServices,

        IOptions<AgentFactorySettings> agentFactorySettings,

        IAgentHubAPIService agentHubService,
        IOptions<AgentHubSettings> agentHubSettings,

        IPromptHubAPIService promptHubService,
        IOptions<PromptHubSettings> promptHubSettings,

        IDataSourceHubAPIService dataSourceHubService,
        IOptions<DataSourceHubSettings> dataSourceHubSettings,

        ILogger<AgentFactoryService> logger,
        IUserIdentityContext userIdentity)
    {
        _orchestrationServices = orchestrationServices;
        
        _agentHubAPIService = agentHubService;
        _agentFactorySettings = agentFactorySettings.Value;
        _agentHubSettings = agentHubSettings.Value;

        _promptHubAPIService = promptHubService;
        _promptHubSettings = promptHubSettings.Value;

        _dataSourceHubAPIService = dataSourceHubService;
        _dataSourceHubSettings = dataSourceHubSettings.Value;

        _logger = logger;

        SetLLMOrchestrationPreference(_agentFactorySettings.DefaultOrchestrationService!);
    }

    /// <summary>
    /// Sets the orchestration service used by the Agent Factory.
    /// </summary>
    /// <param name="orchestrationService"></param>
    /// <returns></returns>
    public bool SetLLMOrchestrationPreference(string orchestrationService)
    {
        if (Enum.TryParse(orchestrationService!, true, out LLMOrchestrationService llmOrchestrationService))
        {
            _llmOrchestrationService = llmOrchestrationService;
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Returns the status of the Semantic kernal.
    /// </summary>
    public string Status
    {
        get
        {
            if (_orchestrationServices.All(os => os.IsInitialized))
                return "ready";

            return string.Join(",", _orchestrationServices
                .Where(os => !os.IsInitialized)
                .Select(os => $"{os.GetType().Name}: initializing"));
        }
    }


    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
    {
        try
        {
            var agent = await AgentBuilder.Build(
                completionRequest.UserPrompt,
                _userIdentity.CurrentUserIdentity.UPN,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService);

            //get prompts for the agent from the prompt hub
            PromptHubResponse promptResponse = await _promptHubService.ResolveRequest(agentResponse.Agent!.Name!, completionRequest.UserContext);

            //get data sources listed for the agent           
            DataSourceHubResponse datasourceResponse = await _dataSourceHubService.ResolveRequest(agentResponse.Agent!.AllowedDataSourceNames!, completionRequest.UserContext);

            if (datasourceResponse != null && datasourceResponse!.DataSources!.Count > 0)
            {
                //construct the configuration
                SQLDatabaseConfiguration dataSourceConfig = new SQLDatabaseConfiguration()
                {
                    Dialect = datasourceResponse.DataSources[0].Dialect,
                    Host = datasourceResponse.DataSources[0].Authentication!["host"],
                    Port = Convert.ToInt32(datasourceResponse.DataSources[0].Authentication!["port"]),
                    DatabaseName = datasourceResponse.DataSources[0].Authentication!["database"],
                    Username = datasourceResponse.DataSources[0].Authentication!["username"],
                    PasswordSecretName = datasourceResponse.DataSources[0].Authentication!["password_secret"],
                    IncludeTables = datasourceResponse.DataSources[0].IncludeTables!,
                    FewShotExampleCount = datasourceResponse.DataSources[0].FewShotExampleCount ?? 0
                };

                //create LLMOrchestrationCompletionRequest
                LLMOrchestrationCompletionRequest llmCompletionRequest = new LLMOrchestrationCompletionRequest()
                {
                    UserPrompt = completionRequest.UserPrompt,
                    Agent = new Agent()
                    {
                        Name = agentResponse.Agent.Name,
                        Type = datasourceResponse.DataSources[0].UnderlyingImplementation,
                        Description = agentResponse.Agent.Description,
                        PromptTemplate = promptResponse.Prompts![0].Prompt
                    },
                    LanguageModel = new LanguageModel()
                    {
                        Type = agentResponse.Agent.LanguageModel!.ModelType,
                        Provider = agentResponse.Agent.LanguageModel.Provider,
                        Temperature = agentResponse.Agent.LanguageModel.Temperature ?? 0f,
                        UseChat = agentResponse.Agent.LanguageModel.UseChat ?? true
                    },
                    DataSource = new SQLDatabaseDataSource()
                    {
                        Name = datasourceResponse.DataSources[0].Name,
                        Type = datasourceResponse.DataSources[0].UnderlyingImplementation,
                        Description = datasourceResponse.DataSources[0].Description,
                        Configuration = dataSourceConfig
                    },
                    MessageHistory = completionRequest.MessageHistory
                };

                // Generate the completion to return to the user
                var result = await GetLLMOrchestrationService().GetCompletion(llmCompletionRequest);

                return new CompletionResponse()
                {
                    Completion = result.Completion!,
                    UserPrompt = completionRequest.UserPrompt,
                    PromptTokens = result.PromptTokens,
                    CompletionTokens = result.CompletionTokens,
                };
            }
            else
                throw new Exception("No datasources returned.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving completion from the orchestration service for {completionRequest.UserPrompt}.");
            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }
    }

    /// <summary>
    /// Retrieve a summarization for the passed in prompt from the orchestration service.
    /// </summary>
    public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
    {
        try
        {
            var agent = await AgentBuilder.Build(
                summaryRequest.UserPrompt,
                _userIdentity.CurrentUserIdentity.UPN,
                _agentHubAPIService,
                _orchestrationServices,
                _promptHubAPIService,
                _dataSourceHubAPIService);

            return await agent.GetSummary(summaryRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving summarization for {summaryRequest.UserPrompt}.");
            return new SummaryResponse
            {
                Summary = "[No Summary]"
            };
        }
    }
}
