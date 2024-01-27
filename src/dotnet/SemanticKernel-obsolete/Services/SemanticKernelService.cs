using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Search;
using FoundationaLLM.SemanticKernel.Chat;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Core.Plugins.Core;
using FoundationaLLM.SemanticKernel.Plugins.Memory;
using FoundationaLLM.SemanticKernel.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Plugins.Memory;
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
    readonly VectorMemoryStore _longTermMemory;
    readonly VectorMemoryStore _shortTermMemory;
    readonly Dictionary<string, Type> _memoryTypes;

    readonly string _shortTermCollectionName = "short-term";

    bool _serviceInitialized = false;
    bool _shortTermMemoryInitialized = false;

    /// <summary>
    /// Flag for the service initialization.
    /// </summary>
    public bool IsInitialized => _serviceInitialized;

    /// <summary>
    /// Constructor for the Semantic Kernel service.
    /// </summary>
    /// <param name="systemPromptService">The System Prompt service.</param>
    /// <param name="memorySources">The list of memory sources.</param>
    /// <param name="options">The configuration options for the Semantic Kernel service.</param>
    /// <param name="cognitiveSearchMemorySourceSettings">The configuration options for the Azure Cognitive Search memory source.</param>
    /// <param name="logger">The logger for the Semantic Kernel service.</param>
    /// <param name="loggerFactory">The logger factory for the Semantic kernel.</param>
    public SemanticKernelService(
        ISystemPromptService systemPromptService,
        IEnumerable<IMemorySource> memorySources,
        IOptions<SemanticKernelServiceSettings> options,
        IOptions<AzureCognitiveSearchMemorySourceSettings> cognitiveSearchMemorySourceSettings,
        ILogger<SemanticKernelService> logger,
        ILoggerFactory loggerFactory)
    {
        _systemPromptService = systemPromptService;
        _memorySources = memorySources;
        _settings = options.Value;
        _logger = logger;
        _logger.LogInformation("Initializing the Semantic Kernel orchestration service...");

        _memoryTypes = ModelRegistry.Models.ToDictionary(m => m.Key, m => m.Value.Type!);

        var builder = new KernelBuilder();

        builder.WithLoggerFactory(loggerFactory);

        builder.WithAzureTextEmbeddingGenerationService(
            _settings.OpenAI.EmbeddingsDeployment,
            _settings.OpenAI.Endpoint,
            _settings.OpenAI.Key);

        builder.WithAzureChatCompletionService(
            _settings.OpenAI.CompletionsDeployment,
            _settings.OpenAI.Endpoint,
            _settings.OpenAI.Key);

        _semanticKernel = builder.Build();

        // The long-term memory uses an Azure Cognitive Search memory store
        _longTermMemory = new VectorMemoryStore(
            _settings.CognitiveSearch.IndexName,
            new AzureCognitiveSearchMemoryStore(
                _settings.CognitiveSearch.Endpoint,
                _settings.CognitiveSearch.Key),
            _semanticKernel.GetService<ITextEmbeddingGeneration>(),
            loggerFactory.CreateLogger<VectorMemoryStore>());

        _shortTermMemory = new VectorMemoryStore(
            _shortTermCollectionName,
            new VolatileMemoryStore(),
            _semanticKernel.GetService<ITextEmbeddingGeneration>(),
            loggerFactory.CreateLogger<VectorMemoryStore>());

        _serviceInitialized = true;

        _logger.LogInformation("Semantic Kernel orchestration service initialized.");
    }

    private async Task EnsureShortTermMemory()
    {
        try
        {
            if (_shortTermMemoryInitialized)
                return;

            // The memories collection in the short term memory store must be created explicitly
            await _shortTermMemory.MemoryStore.CreateCollectionAsync(_shortTermCollectionName);

            // Get current short term memories. Short term memories are generated or loaded at runtime and kept in SK's volatile memory.
            //The memories (data) here were generated from ACSMemorySourceConfig.json in blob storage that was used to execute faceted queries in Cog Search to iterate through
            //each product category stored and count up the number of products in each category. The query also counts all the products for the entire company.
            //The content here has embeddings generated on it so it can be used in a vector query by the user

            // TODO: Explore the option of moving static memories loaded from blob storage into the long-term memory (e.g., the Azure Cognitive Search index).
            // For now, the static memories are re-loaded each time together with the analytical short-term memories originating from Azure Cognitive Search faceted queries.
            var shortTermMemories = new List<string>();
            foreach (var memorySource in _memorySources)
            {
                shortTermMemories.AddRange(await memorySource.GetMemories());
            }

            foreach (var stm in shortTermMemories
                .Select(m => (object)new ShortTermMemory
                {
                    entityType__ = nameof(ShortTermMemory),
                    memory__ = m
                }))
            {
                await _shortTermMemory.AddMemory(stm, "N/A");
            }

            _shortTermMemoryInitialized = true;
            _logger.LogInformation("Semantic Kernel service short-term memory initialized.");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The Semantic Kernel service short-term memory failed to initialize.");
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
        await EnsureShortTermMemory();

        var memoryPlugin = new TextEmbeddingObjectMemoryPlugin(
            _longTermMemory,
            _shortTermMemory,
            _logger);

        var memories = await memoryPlugin.RecallAsync(
            userPrompt,
            _settings.CognitiveSearch.IndexName,
            0.8,
            _settings.CognitiveSearch.MaxVectorSearchResults);

        // Read the resulting user prompt embedding as soon as possible
        var userPromptEmbedding = memoryPlugin.LastInputTextEmbedding?.ToArray();

        List<string> memoryCollection;
        if (string.IsNullOrEmpty(memories))
            memoryCollection = new List<string>();
        else
            memoryCollection = JsonConvert.DeserializeObject<List<string>>(memories)!;

        var chatHistory = new ChatBuilder(
                _semanticKernel,
                _settings.OpenAI.CompletionsDeploymentMaxTokens,
                _memoryTypes,
                promptOptimizationSettings: _settings.OpenAI.PromptOptimization)
            .WithSystemPrompt(
                await _systemPromptService.GetPrompt(_settings.OpenAI.ChatCompletionPromptName))
            .WithMemories(
                memoryCollection!)
            .WithMessageHistory(
                messageHistory.Select(m => (new AuthorRole(m.Sender.ToLower()), m.Text.NormalizeLineEndings())).ToList())
            .Build();

        chatHistory.AddUserMessage(userPrompt);

        var chat = _semanticKernel.GetService<IChatCompletion>();
        var completionResults = await chat.GetChatCompletionsAsync(chatHistory);

        // TODO: Add validation and perhaps fall back to a standard response if no completions are generated.
        var reply = await completionResults[0].GetChatMessageAsync();
        var rawResult = (completionResults[0] as ITextResult)!.ModelResult.GetOpenAIChatResult();

        return reply.Content;
    }

    /// <summary>
    /// Gets a summary from the Semantic Kernel service.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <returns>The prompt summary.</returns>
    public async Task<string> GetSummary(string userPrompt)
    {
        var summarizerPlugin = new TextSummaryPlugin(
                    await _systemPromptService.GetPrompt(_settings.OpenAI.ShortSummaryPromptName),
                    500,
                    _semanticKernel);

        var updatedContext = await summarizerPlugin.SummarizeTextAsync(
            userPrompt);

        //Remove all non-alpha numeric characters (Turbo has a habit of putting things in quotes even when you tell it not to)
        var summary = Regex.Replace(updatedContext, @"[^a-zA-Z0-9.\s]", "");

        return summary;
    }

    /// <summary>
    /// Add an object instance and its associated vectorization to the underlying memory.
    /// </summary>
    /// <param name="item">The object instance to be added to the memory.</param>
    /// <param name="itemName">The name of the object instance.</param>
    /// <returns></returns>
    public async Task AddMemory(object item, string itemName)
    {
        await _longTermMemory.AddMemory(item, itemName);
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
