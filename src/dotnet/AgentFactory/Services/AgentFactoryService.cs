using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Services;

public class AgentFactoryService : IAgentFactoryService
{
    private readonly ISemanticKernelOrchestrationService _semanticKernelOrchestration;
    private readonly ILangChainOrchestrationService _langChainOrchestration;
    private readonly ChatServiceSettings _settings;
    private readonly ILogger<AgentFactoryService> _logger;

    private LLMOrchestrationService _llmOrchestrationService = LLMOrchestrationService.LangChain;

    public AgentFactoryService(
        ISemanticKernelOrchestrationService semanticKernelOrchestration,
        ILangChainOrchestrationService langChainOrchestration,
        IOptions<ChatServiceSettings> options,
        ILogger<AgentFactoryService> logger)
    {
        _semanticKernelOrchestration = semanticKernelOrchestration;
        _langChainOrchestration = langChainOrchestration;
        _settings = options.Value;
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
    public async Task<CompletionResponseBase> GetCompletion(CompletionRequestBase completionRequest)
    {
        try
        {
            // Generate the completion to return to the user
            var result = await GetLLMOrchestrationService().GetResponse(completionRequest.Prompt,
                completionRequest.MessageHistory);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving completion from the orchestration service for {completionRequest.Prompt}.");
            return new CompletionResponseBase
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
    public async Task<string> GetSummary(string content)
    {
        try
        {
            var summary = await GetLLMOrchestrationService().Summarize(content);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving summarization for {content}.");
            return "[No Summary]";
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
