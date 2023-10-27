using FoundationaLLM.Common.Constants;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using Microsoft.Extensions.Logging;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Configuration.Branding;
using Microsoft.Extensions.Options;
using FoundationaLLM.Core.Models;

namespace FoundationaLLM.Core.Services;

/// <ineritdoc/>
public class CoreService : ICoreService
{
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IGatekeeperAPIService _gatekeeperAPIService;
    private readonly ILogger<CoreService> _logger;
    private readonly string _sessionType;

    /// <summary>
    /// Indicates whether the service is ready to accept requests.
    /// </summary>
    public string Status
    {
        get
        {
            if (_cosmosDbService.IsInitialized)
                return "ready";

            var status = new List<string>();

            if (!_cosmosDbService.IsInitialized)
                status.Add("CosmosDBService: initializing");

            return string.Join(",", status);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreService"/> class.
    /// </summary>
    /// <param name="cosmosDbService">The Azure Cosmos DB service that contains
    /// chat sessions and messages.</param>
    /// <param name="gatekeeperAPIService">The service used to make calls to
    /// the Gatekeeper API.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="CoreService"/> type name.</param>
    /// <param name="settings">The <see cref="ClientBrandingConfiguration"/>
    /// settings retrieved by the injected <see cref="IOptions{TOptions}"/>.</param>
    public CoreService(
        ICosmosDbService cosmosDbService,
        IGatekeeperAPIService gatekeeperAPIService,
        ILogger<CoreService> logger,
        IOptions<ClientBrandingConfiguration> settings)
    {
        _cosmosDbService = cosmosDbService;
        _gatekeeperAPIService = gatekeeperAPIService;
        _logger = logger;
        _sessionType = settings.Value.KioskMode ? SessionTypes.KioskSession : SessionTypes.Session;
    }

    /// <summary>
    /// Returns list of chat session ids and names.
    /// </summary>
    public async Task<List<Session>> GetAllChatSessionsAsync()
    {
        return await _cosmosDbService.GetSessionsAsync(_sessionType);
    }

    /// <summary>
    /// Returns the chat messages related to an existing session.
    /// </summary>
    public async Task<List<Message>> GetChatSessionMessagesAsync(string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        return await _cosmosDbService.GetSessionMessagesAsync(sessionId);
    }

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    public async Task<Session> CreateNewChatSessionAsync()
    {
        Session session = new();
        session.Type = _sessionType;
        return await _cosmosDbService.InsertSessionAsync(session);
    }

    /// <summary>
    /// Rename the chat session from its default (eg., "New Chat") to the summary provided by OpenAI.
    /// </summary>
    public async Task<Session> RenameChatSessionAsync(string sessionId, string newChatSessionName)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(newChatSessionName);

        return await _cosmosDbService.UpdateSessionNameAsync(sessionId, newChatSessionName);
    }

    /// <summary>
    /// Delete a chat session and related messages.
    /// </summary>
    public async Task DeleteChatSessionAsync(string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        await _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId);
    }

    /// <summary>
    /// Receive a prompt from a user, vectorize it from the OpenAI service, and get a completion from the OpenAI service.
    /// </summary>
    public async Task<Completion> GetChatCompletionAsync(string? sessionId, string userPrompt)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sessionId);

            // Retrieve conversation, including latest prompt.
            // If you put this after the vector search it doesn't take advantage of previous information given so harder to chain prompts together.
            // However if you put this before the vector search it can get stuck on previous answers and not pull additional information. Worth experimenting

            // Retrieve conversation, including latest prompt.
            var messages = await _cosmosDbService.GetSessionMessagesAsync(sessionId);
            var messageHistoryList = messages
                .Select(message => new MessageHistoryItem(message.Sender, message.Text))
                .ToList();

            var completionRequest = new CompletionRequest
            {
                UserPrompt = userPrompt,
                MessageHistory = messageHistoryList
            };

            // Generate the completion to return to the user.
            var result = await _gatekeeperAPIService.GetCompletion(completionRequest);

            // Add to prompt and completion to cache, then persist in Cosmos as transaction.
            var promptMessage = new Message(sessionId, nameof(Participants.User), result.PromptTokens, userPrompt, result.UserPromptEmbedding, null);
            var completionMessage = new Message(sessionId, nameof(Participants.Assistant), result.CompletionTokens, result.Completion, null, null);
            var completionPrompt = new CompletionPrompt(sessionId, completionMessage.Id, result.UserPrompt);
            completionMessage.CompletionPromptId = completionPrompt.Id;

            await AddPromptCompletionMessagesAsync(sessionId, promptMessage, completionMessage, completionPrompt);

            return new Completion { Text = result.Completion };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion in session {sessionId} for user prompt [{userPrompt}].");
            return new Completion { Text = "Could not generate a completion due to an internal error." };
        }
    }

    /// <summary>
    /// Generate a name for a chat message, based on the passed in prompt.
    /// </summary>
    public async Task<Completion> SummarizeChatSessionNameAsync(string? sessionId, string prompt)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(sessionId);

            await Task.CompletedTask;

            var summary = await _gatekeeperAPIService.GetSummary(prompt);

            await RenameChatSessionAsync(sessionId, summary);

            return new Completion { Text = summary };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting a summary in session {sessionId} for user prompt [{prompt}].");
            return new Completion { Text = "[No Summary]" };
        }
    }

    /// <summary>
    /// Add a new user prompt to the chat session and insert into the data service.
    /// </summary>
    private async Task<Message> AddPromptMessageAsync(string sessionId, string promptText)
    {
        Message promptMessage = new(sessionId, nameof(Participants.User), default, promptText, null, null);

        return await _cosmosDbService.InsertMessageAsync(promptMessage);
    }


    /// <summary>
    /// Add user prompt and AI assistance response to the chat session message list object and insert into the data service as a transaction.
    /// </summary>
    private async Task AddPromptCompletionMessagesAsync(string sessionId, Message promptMessage, Message completionMessage, CompletionPrompt completionPrompt)
    {
        var session = await _cosmosDbService.GetSessionAsync(sessionId);

        // Update session cache with tokens used.
        session.TokensUsed += promptMessage.Tokens;
        session.TokensUsed += completionMessage.Tokens;

        await _cosmosDbService.UpsertSessionBatchAsync(promptMessage, completionMessage, completionPrompt, session);
    }

    /// <summary>
    /// Rate an assistant message. This can be used to discover useful AI responses for training, discoverability, and other benefits down the road.
    /// </summary>
    public async Task<Message> RateMessageAsync(string id, string sessionId, bool? rating)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(sessionId);

        return await _cosmosDbService.UpdateMessageRatingAsync(id, sessionId, rating);
    }

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNullOrEmpty(completionPromptId);

        return await _cosmosDbService.GetCompletionPrompt(sessionId, completionPromptId);
    }
}