using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Core.Services;

/// <ineritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="CoreService"/> class.
/// </summary>
/// <param name="cosmosDbService">The Azure Cosmos DB service that contains
/// chat sessions and messages.</param>
/// <param name="downstreamAPIServices">The services used to make calls to
/// the downstream APIs.</param>
/// <param name="logger">The logging interface used to log under the
/// <see cref="CoreService"/> type name.</param>
/// <param name="brandingSettings">The <see cref="ClientBrandingConfiguration"/>
/// settings retrieved by the injected <see cref="IOptions{TOptions}"/>.</param>
/// <param name="settings">The <see cref="CoreServiceSettings"/> settings for the service.</param>
/// <param name="callContext">Contains contextual data for the calling service.</param>
/// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
public partial class CoreService(
    ICosmosDbService cosmosDbService,
    IEnumerable<IDownstreamAPIService> downstreamAPIServices,
    ILogger<CoreService> logger,
    IOptions<ClientBrandingConfiguration> brandingSettings,
    IOptions<CoreServiceSettings> settings,
    ICallContext callContext,
    IEnumerable<IResourceProviderService> resourceProviderServices) : ICoreService
{
    private readonly ICosmosDbService _cosmosDbService = cosmosDbService;
    private readonly IDownstreamAPIService _gatekeeperAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClients.GatekeeperAPI);
    private readonly IDownstreamAPIService _orchestrationAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClients.OrchestrationAPI);
    private readonly ILogger<CoreService> _logger = logger;
    private readonly ICallContext _callContext = callContext;
    private readonly string _sessionType = brandingSettings.Value.KioskMode ? SessionTypes.KioskSession : SessionTypes.Session;
    private readonly CoreServiceSettings _settings = settings.Value;
    private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
        resourceProviderServices.ToDictionary<IResourceProviderService, string>(
            rps => rps.Name);

    /// <summary>
    /// Returns list of chat session ids and names.
    /// </summary>
    public async Task<List<Session>> GetAllChatSessionsAsync() =>
        await _cosmosDbService.GetSessionsAsync(_sessionType, _callContext.CurrentUserIdentity?.UPN ?? 
                                                              throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat sessions."));

    /// <summary>
    /// Returns the chat messages related to an existing session.
    /// </summary>
    public async Task<List<Message>> GetChatSessionMessagesAsync(string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        return await _cosmosDbService.GetSessionMessagesAsync(sessionId, _callContext.CurrentUserIdentity?.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));
    }

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    public async Task<Session> CreateNewChatSessionAsync()
    {
        Session session = new()
        {
            Type = _sessionType,
            UPN = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when creating a new chat session.")
        };
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
    /// Receive a prompt from a user, retrieve the message history from the related session,
    /// generate a completion response, and log full completion results.
    /// </summary>
    public async Task<Completion> GetChatCompletionAsync(CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);

            // Retrieve conversation, including latest prompt.
            var messages = await _cosmosDbService.GetSessionMessagesAsync(completionRequest.SessionId, _callContext.CurrentUserIdentity?.UPN ??
                throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat completions."));
            var messageHistoryList = messages
                .Select(message => new MessageHistoryItem(message.Sender, string.IsNullOrWhiteSpace(message.Text) ? "" : message.Text))
                .ToList();

            completionRequest.MessageHistory = messageHistoryList;

            var agentOption = await ProcessGatekeeperOptions(completionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(completionRequest);

            // Add to prompt and completion to cache, then persist in Cosmos as transaction.
            // Add the user's UPN to the messages.
            var upn = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when adding prompt and completion messages.");
            var promptMessage = new Message(completionRequest.SessionId, nameof(Participants.User), result.PromptTokens, completionRequest.UserPrompt, result.UserPromptEmbedding, null, upn, _callContext.CurrentUserIdentity?.Name);
            var completionMessage = new Message(completionRequest.SessionId, nameof(Participants.Assistant), result.CompletionTokens, result.Completion, null, null, upn, result.AgentName, result.Citations);
            var completionPromptText =
                $"User prompt: {result.UserPrompt}{Environment.NewLine}Agent: {result.AgentName}{Environment.NewLine}Prompt template: {(!string.IsNullOrWhiteSpace(result.FullPrompt) ? result.FullPrompt : result.PromptTemplate)}";
            var completionPrompt = new CompletionPrompt(completionRequest.SessionId, completionMessage.Id, completionPromptText);
            completionMessage.CompletionPromptId = completionPrompt.Id;

            await AddPromptCompletionMessagesAsync(completionRequest.SessionId, promptMessage, completionMessage, completionPrompt);

            return new Completion { Text = result.Completion };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion in session {SessionId} for user prompt [{UserPrompt}].",
                completionRequest.SessionId, completionRequest.UserPrompt);
            return new Completion { Text = "Could not generate a completion due to an internal error." };
        }
    }

    /// <summary>
    /// Provides a completion for a user prompt, without a session.
    /// </summary>
    public async Task<Completion> GetCompletionAsync(CompletionRequest directCompletionRequest)
    {
        try
        {
            var agentOption = await ProcessGatekeeperOptions(directCompletionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(directCompletionRequest);

            return new Completion { Text = result.Completion };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion for user prompt [{directCompletionRequest.UserPrompt}].");
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

            var sessionNameSummary = string.Empty;

            switch (_settings.SessionSummarization)
            {
                case ChatSessionNameSummarizationType.Timestamp:
                    sessionNameSummary = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm}";
                    break;
                case ChatSessionNameSummarizationType.LLM:
                    var summaryRequest = new SummaryRequest()
                    {
                        SessionId = sessionId,
                        UserPrompt = prompt
                    };

                    var summaryResponse = await GetDownstreamAPIService(AgentGatekeeperOverrideOption.UseSystemOption).GetSummary(summaryRequest);

                    // Remove any punctuation from the summary.
                    sessionNameSummary = ChatSessionNameReplacementRegex().Replace(summaryResponse.Summary!, string.Empty);
                    break;
                default:
                    throw new Exception($"The chat session summarization type {_settings.SessionSummarization} is not supported.");
            }

            await RenameChatSessionAsync(sessionId, sessionNameSummary);

            return new Completion { Text = sessionNameSummary };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting a summary in session {sessionId} for user prompt [{prompt}].");
            return new Completion { Text = "[No Summary]" };
        }
    }

    private IDownstreamAPIService GetDownstreamAPIService(AgentGatekeeperOverrideOption agentOption) =>
        ((agentOption == AgentGatekeeperOverrideOption.UseSystemOption) && _settings.BypassGatekeeper)
        || (agentOption == AgentGatekeeperOverrideOption.MustBypass)
            ? _orchestrationAPIService
            : _gatekeeperAPIService;

    private async Task<AgentGatekeeperOverrideOption> ProcessGatekeeperOptions(CompletionRequest completionRequest)
    {
        if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
            throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");

        var agentBase = await agentResourceProvider.GetResource<AgentBase>($"/{AgentResourceTypeNames.Agents}/{completionRequest.AgentName}", _callContext.CurrentUserIdentity ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the agent settings."));

        if (agentBase?.Gatekeeper?.UseSystemSetting == false)
        {
            // Agent does not want to use system settings, however it does not have any Gatekeeper options either
            // Consequently, a request to bypass Gatekeeper will be returned.
            if (agentBase!.Gatekeeper!.Options == null || agentBase.Gatekeeper.Options.Length == 0)
                return AgentGatekeeperOverrideOption.MustBypass;

            completionRequest.GatekeeperOptions = agentBase.Gatekeeper.Options;
            return AgentGatekeeperOverrideOption.MustCall;
        }

        return AgentGatekeeperOverrideOption.UseSystemOption;
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
        // Add the user's UPN to the messages.
        var upn = _callContext.CurrentUserIdentity?.UPN ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when adding prompt and completion messages.");
        promptMessage.UPN = upn;
        completionMessage.UPN = upn;

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

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex ChatSessionNameReplacementRegex();
}
