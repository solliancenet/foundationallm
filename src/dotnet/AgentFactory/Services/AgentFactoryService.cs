using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.AgentFactory.Core.Services;

public class AgentFactoryService : IAgentFactoryService
{
    private readonly ISemanticKernelOrchestrationService _semanticKernelOrchestration;
    private readonly ILangChainOrchestrationService _langChainOrchestration;
    private readonly IAgentHubService _agentHubService;
    private readonly AgentHubSettings _settings;
    private readonly ILogger<AgentFactoryService> _logger;

    private LLMOrchestrationService _llmOrchestrationService = LLMOrchestrationService.LangChain;

    public AgentFactoryService(
        ISemanticKernelOrchestrationService semanticKernelOrchestration,
        ILangChainOrchestrationService langChainOrchestration,
        IAgentHubService agentHubService,
        IOptions<AgentHubSettings> settings,
        ILogger<AgentFactoryService> logger)
    {
        _semanticKernelOrchestration = semanticKernelOrchestration;
        _langChainOrchestration = langChainOrchestration;
        _agentHubService = agentHubService;
        _settings = settings.Value;
        _logger = logger;

        SetLLMOrchestrationPreference(_settings.DefaultOrchestrationService);
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
            if (_semanticKernelOrchestration.IsInitialized)
                return "ready";
            var status = new List<string>();
            if (!_semanticKernelOrchestration.IsInitialized)
                status.Add("SemanticKernelOrchestrationService: initializing");
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
            //get all agents for prompt...
            List<AgentHubResponse> agents = await _agentHubService.ResolveRequest(completionRequest.Prompt, "");

            // Generate the completion to return to the user
            var result = await GetLLMOrchestrationService().GetResponse(completionRequest.Prompt,
                completionRequest.MessageHistory);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving completion from the orchestration service for {completionRequest.Prompt}.");
            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.Prompt,
                UserPromptTokens = 0,
                ResponseTokens = 0,
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
                return _semanticKernelOrchestration as ILLMOrchestrationService;
            case LLMOrchestrationService.LangChain:
            default:
                return _langChainOrchestration as ILLMOrchestrationService;
        }
    }
}
