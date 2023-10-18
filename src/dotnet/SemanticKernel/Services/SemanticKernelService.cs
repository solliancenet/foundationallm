using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Search;
using FoundationaLLM.SemanticKernel.Chat;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Memory;
using FoundationaLLM.SemanticKernel.Memory.AzureCognitiveSearch;
using FoundationaLLM.SemanticKernel.Skills.Core;
using FoundationaLLM.SemanticKernel.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace FoundationaLLM.SemanticKernel.Core.Services;

/// <summary>
/// Implements the <see cref="ISemanticKernelService"/> interface.
/// </summary>
public class SemanticKernelService : ISemanticKernelService
{
    readonly SemanticKernelServiceSettings _settings;

    readonly IKernel _semanticKernel;
    readonly IEnumerable<IMemorySource> _memorySources;
    readonly ILogger<SemanticKernelService> _logger;
    readonly ISystemPromptService _systemPromptService;
    readonly IChatCompletion _chat;
    readonly AzureCognitiveSearchVectorMemory _longTermMemory;
    readonly ShortTermVolatileMemoryStore _shortTermMemory;
    readonly Dictionary<string, Type> _memoryTypes;

    bool _memoryInitialized = false;

    /// <summary>
    /// Flag for the memory stores initialization.
    /// </summary>
    public bool IsInitialized => _memoryInitialized;

    /// <summary>
    /// Constructor for the Semantic Kernel service.
    /// </summary>
    /// <param name="systemPromptService">The System Prompt service.</param>
    /// <param name="memorySources">The list of memory sources.</param>
    /// <param name="options">The configuration options for the Semantic Kernel service.</param>
    /// <param name="cognitiveSearchMemorySourceSettings">The configuration options for the Azure Cognitive Search memory source.</param>
    /// <param name="logger">The logger for the Semantic Kernel service.</param>
    /// <param name="skLogger">The logger for the Semantic kernel.</param>
    public SemanticKernelService(
        ISystemPromptService systemPromptService,
        IEnumerable<IMemorySource> memorySources,
        IOptions<SemanticKernelServiceSettings> options,
        IOptions<AzureCognitiveSearchMemorySourceSettings> cognitiveSearchMemorySourceSettings,
        ILogger<SemanticKernelService> logger,
        ILogger<Kernel> skLogger)
    {
        _systemPromptService = systemPromptService;
        _memorySources = memorySources;
        _settings = options.Value;
        _logger = logger;
        _logger.LogInformation("Initializing the Semantic Kernel orchestration service...");

        _memoryTypes = ModelRegistry.Models.ToDictionary(m => m.Key, m => m.Value.Type);

        var builder = new KernelBuilder();

        builder.WithLogger(skLogger);

        builder.WithAzureTextEmbeddingGenerationService(
            _settings.OpenAI.EmbeddingsDeployment,
            _settings.OpenAI.Endpoint,
            _settings.OpenAI.Key);

        builder.WithAzureChatCompletionService(
            _settings.OpenAI.CompletionsDeployment,
            _settings.OpenAI.Endpoint,
            _settings.OpenAI.Key);

        _semanticKernel = builder.Build();

        _longTermMemory = new AzureCognitiveSearchVectorMemory(
            _settings.CognitiveSearch.Endpoint,
            _settings.CognitiveSearch.Key,
            _settings.CognitiveSearch.IndexName,
            _semanticKernel.GetService<ITextEmbeddingGeneration>(),
            _logger);

        _shortTermMemory = new ShortTermVolatileMemoryStore(
            _semanticKernel.GetService<ITextEmbeddingGeneration>(),
            _logger);

        Task.Run(() =>  InitializeMemory());

        _semanticKernel.RegisterMemory(_longTermMemory);

        _chat = _semanticKernel.GetService<IChatCompletion>();
        _logger.LogInformation("Semantic Kernel orchestration service initialized.");
    }

    /// <summary>
    /// Initialize the long-term and the short-term memory stores.
    /// </summary>
    /// <returns></returns>
    private async Task InitializeMemory()
    {
        try
        {
            _logger.LogInformation("Initializing the Semantic Kernel orchestration service memory...");
            await _longTermMemory.Initialize(_memoryTypes.Values.ToList());

            // Get current short term memories
            // TODO: Explore the option of moving static memories loaded from blob storage into the long-term memory (e.g., the Azure Cognitive Search index).
            // For now, the static memories are re-loaded each time together with the analytical short-term memories originating from Azure Cognitive Search faceted queries.
            var shortTermMemories = new List<string>(); 
            foreach (var memorySource in _memorySources)
            {
                shortTermMemories.AddRange(await memorySource.GetMemories());
            }

            await _shortTermMemory.Initialize(shortTermMemories
                .Select(m => (object) new ShortTermMemory
                {
                    entityType__ = nameof(ShortTermMemory),
                    memory__ = m
                }).ToList());

            _memoryInitialized = true;
            _logger.LogInformation("Semantic Kernel orchestration service memory initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Semantic Kernel orchestration service memory failed to initialize.");
        }
    }

    /// <summary>
    /// Gets a completion from the Semantic Kernel service.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <param name="messageHistory">A list of previous messages.</param>
    /// <returns>The completion text.</returns>
    public async Task<string> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory)
    {
        var memorySkill = new TextEmbeddingObjectMemorySkill(
            _longTermMemory,
            _shortTermMemory,
            _logger);

        var memories = await memorySkill.RecallAsync(
            userPrompt,
            _settings.CognitiveSearch.IndexName,
            0.8,
            _settings.CognitiveSearch.MaxVectorSearchResults);

        // Read the resulting user prompt embedding as soon as possile
        var userPromptEmbedding = memorySkill.LastInputTextEmbedding?.ToArray();

        var memoryCollection = new List<string>();

        if (!string.IsNullOrEmpty(memories))
            memoryCollection = JsonConvert.DeserializeObject<List<string>>(memories);

        var chatHistory = new ChatBuilder(
                _semanticKernel,
                _settings.OpenAI.CompletionsDeploymentMaxTokens,
                _memoryTypes,
                promptOptimizationSettings: _settings.OpenAI.PromptOptimization)
            .WithSystemPrompt(
                await _systemPromptService.GetPrompt(_settings.OpenAI.ChatCompletionPromptName))
            .WithMemories(
                memoryCollection ?? new List<string>())
            .WithMessageHistory(
                messageHistory.Select(m => (new AuthorRole(m.Sender.ToLower()), m.Text.NormalizeLineEndings())).ToList())
            .Build();

        chatHistory.AddUserMessage(userPrompt);

        var chat = _semanticKernel.GetService<IChatCompletion>();
        var completionResults = await chat.GetChatCompletionsAsync(chatHistory);

        // TODO: Add validation and perhaps fall back to a standard response if no completions are generated.
        var reply = await completionResults[0].GetChatMessageAsync();
        var rawResult = (completionResults[0] as ITextResult).ModelResult.GetOpenAIChatResult();

        var completion = new CompletionResponse(reply.Content, chatHistory[0].Content, rawResult.Usage.PromptTokens,
            rawResult.Usage.CompletionTokens, userPromptEmbedding);

        return completion.Completion;
    }

    /// <summary>
    /// Gets a summary from the Semantic Kernel service.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <returns>The prompt summary.</returns>
    public async Task<string> GetSummary(string userPrompt)
    {
        var summarizerSkill = new GenericSummarizerSkill(
            await _systemPromptService.GetPrompt(_settings.OpenAI.ShortSummaryPromptName),
            500,
            _semanticKernel);

        var updatedContext = await summarizerSkill.SummarizeConversationAsync(
            userPrompt,
            _semanticKernel.CreateNewContext());

        //Remove all non-alpha numeric characters (Turbo has a habit of putting things in quotes even when you tell it not to
        var summary = Regex.Replace(updatedContext.Result, @"[^a-zA-Z0-9\s]", "");

        return summary;
    }

    /// <summary>
    /// Add an object instance and its associated vectorization to the underlying memory.
    /// </summary>
    /// <param name="item">The object instance to be added to the memory.</param>
    /// <param name="itemName">The name of the object instance.</param>
    /// <param name="vectorizer">The logic that sets the embedding vector as a property on the object.</param>
    /// <returns></returns>
    public async Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer)
    {
        if (item is EmbeddedEntity entity)
            entity.entityType__ = item.GetType().Name;
        else
            throw new ArgumentException("Only objects derived from EmbeddedEntity can be added to memory.");

        await _longTermMemory.AddMemory(item, itemName, vectorizer);
    }

    /// <summary>
    /// Removes an object instance and its associated vectorization from the underlying memory.
    /// </summary>
    /// <param name="item">The object instance to be removed from the memory.</param>
    /// <returns></returns>
    public async Task RemoveMemory(object item)
    {
        await _longTermMemory.RemoveMemory(item);
    }
}
