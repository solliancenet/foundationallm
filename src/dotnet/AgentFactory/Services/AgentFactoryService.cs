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

namespace FoundationaLLM.AgentFactory.Core.Services;

public class AgentFactoryService : IAgentFactoryService
{
    private readonly ISemanticKernelService _semanticKernel;
    private readonly ILangChainService _langChain;
    private readonly IAgentHubService _agentHubService;
    private readonly AgentFactorySettings _agentFactorySettings;
    private readonly AgentHubSettings _agentHubSettings;
    
    private readonly PromptHubSettings _promptHubSettings;
    private readonly IPromptHubService _promptHubService;

    private readonly DataSourceHubSettings _dataSourceHubSettings;
    private readonly IDataSourceHubService _dataSourceHubService;

    private readonly ILogger<AgentFactoryService> _logger;

    private LLMOrchestrationService _llmOrchestrationService = LLMOrchestrationService.LangChain;

    public AgentFactoryService(
        ISemanticKernelService semanticKernel,
        ILangChainService langChain,
        
        IAgentHubService agentHubService,
        IOptions<AgentFactorySettings> agentFactorySettings,
        IOptions<AgentHubSettings> agentHubSettings,

        IPromptHubService promptHubService,
        IOptions<PromptHubSettings> promptHubSettings,

        IDataSourceHubService dataSourceHubService,
        IOptions<DataSourceHubSettings> dataSourceHubSettings,

        ILogger<AgentFactoryService> logger)
    {
        _semanticKernel = semanticKernel;
        _langChain = langChain;
        
        _agentHubService = agentHubService;
        _agentFactorySettings = agentFactorySettings.Value;
        _agentHubSettings = agentHubSettings.Value;

        _promptHubService = promptHubService;
        _promptHubSettings = promptHubSettings.Value;

        _dataSourceHubService = dataSourceHubService;
        _dataSourceHubSettings = dataSourceHubSettings.Value;

        _logger = logger;

        SetLLMOrchestrationPreference(_agentFactorySettings.DefaultOrchestrationService);
    }

    public bool SetLLMOrchestrationPreference(string orchestrationService)
    {
        if (Enum.TryParse(orchestrationService, true, out LLMOrchestrationService llmOrchestrationService))
        {
            _llmOrchestrationService = llmOrchestrationService;
            return true;
        }
        else
            return false;
    }
    public string Status
    {
        get
        {
            if (_semanticKernel.IsInitialized)
                return "ready";
            var status = new List<string>();
            if (!_semanticKernel.IsInitialized)
                status.Add("SemanticKernelService: initializing");
            return string.Join(",", status);
        }
    }


    /// <summary>
    /// Retrieve a completion from the configured orchestration service.
    /// </summary>
    public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
    {
        try
        {
            //get agent for prompt...
            AgentHubResponse agentResponse= await _agentHubService.ResolveRequest(completionRequest.UserPrompt, completionRequest.UserContext);

            //get prompts for the agent from the prompt hub
            PromptHubResponse promptResponse = await _promptHubService.ResolveRequest(agentResponse.Agent.Name, completionRequest.UserContext);

            //get data sources listed for the agent           
            DataSourceHubResponse datasourceResponse = await _dataSourceHubService.ResolveRequest(agentResponse.Agent.AllowedDataSourceNames, completionRequest.UserContext);
            //construct the configuration
            SQLDatabaseConfiguration dataSourceConfig = new SQLDatabaseConfiguration()
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

            //create LLMOrchestrationCompletionRequest
            LLMOrchestrationCompletionRequest llmCompletionRequest = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = completionRequest.UserPrompt,
                Agent = new Agent()
                {
                    Name = agentResponse.Agent.Name,
                    Type = datasourceResponse.DataSources[0].UnderlyingImplementation,
                    Description =  agentResponse.Agent.Description,
                    PromptTemplate = promptResponse.Prompts[0].Prompt                    
                },
                LanguageModel = new LanguageModel()
                {
                    Type = agentResponse.Agent.LanguageModel.ModelType,
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
                Completion = result.Completion,
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
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
    public async Task<SummaryResponse> GetSummary(SummaryRequest content)
    {
        try
        {
            var summary = await GetLLMOrchestrationService().GetSummary(content.Prompt);

            return new SummaryResponse
            {
                Info = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving summarization for {content}.");
            return new SummaryResponse
            {
                Info = "[No Summary]"
            };
        }
    }
    private ILLMOrchestrationService GetLLMOrchestrationService()
    {
        switch (_llmOrchestrationService)
        {
            case LLMOrchestrationService.SemanticKernel:
                return _semanticKernel as ILLMOrchestrationService;
            case LLMOrchestrationService.LangChain:
            default:
                return _langChain as ILLMOrchestrationService;
        }
    }
}
