using System.Text;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Orchestration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.Orchestration.Response.OpenAI;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using Conversation = FoundationaLLM.Common.Models.Conversation.Conversation;
using LongRunningOperation = FoundationaLLM.Common.Models.Orchestration.LongRunningOperation;
using Message = FoundationaLLM.Common.Models.Conversation.Message;

namespace FoundationaLLM.Core.Services;

/// <ineritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="CoreService"/> class.
/// </summary>
/// <param name="cosmosDBService">The Azure Cosmos DB service that contains
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
/// <param name="configuration">The <see cref="IConfiguration"/> service providing configuration settings.</param>
public partial class CoreService(
    IAzureCosmosDBService cosmosDBService,
    IEnumerable<IDownstreamAPIService> downstreamAPIServices,
    ILogger<CoreService> logger,
    IOptions<ClientBrandingConfiguration> brandingSettings,
    IOptions<CoreServiceSettings> settings,
    ICallContext callContext,
    IEnumerable<IResourceProviderService> resourceProviderServices,
    IConfiguration configuration,
    IHttpClientFactoryService httpClientFactory) : ICoreService
{
    private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
    private readonly IDownstreamAPIService _gatekeeperAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.GatekeeperAPI);
    private readonly IDownstreamAPIService _orchestrationAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.OrchestrationAPI);
    private readonly ILogger<CoreService> _logger = logger;
    private readonly UnifiedUserIdentity _userIdentity = ValidateUserIdentity(callContext.CurrentUserIdentity);
    private readonly string _sessionType = brandingSettings.Value.KioskMode ? ConversationTypes.KioskSession : ConversationTypes.Session;
    private readonly CoreServiceSettings _settings = settings.Value;
    private readonly IHttpClientFactoryService _httpClientFactory = httpClientFactory;
    private readonly string _baseUrl = GetBaseUrl(configuration, httpClientFactory, callContext).GetAwaiter().GetResult();

    private readonly IResourceProviderService _attachmentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
    private readonly IResourceProviderService _agentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
    private readonly IResourceProviderService _azureOpenAIResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureOpenAI);
    private readonly IResourceProviderService _aiModelResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AIModel);
    private readonly IResourceProviderService _configurationResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration);
    private readonly IResourceProviderService _conversationResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Conversation);

    private readonly HashSet<string> _azureOpenAIFileSearchFileExtensions =
        settings.Value.AzureOpenAIAssistantsFileSearchFileExtensions
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.ToLowerInvariant())
            .ToHashSet();

    #region Conversation management - FoundationaLLM.Conversation resource provider

    /// <inheritdoc/>
    public async Task<List<Conversation>> GetAllConversationsAsync(string instanceId)
    {
        var result = await _conversationResourceProvider.GetResourcesAsync<Conversation>(
            instanceId,
            _userIdentity,
            new()
            {
                Parameters = new()
                {
                    { ConversationResourceProviderGetParameterNames.ConversationType, _sessionType }
                }
            });

        return result.Select(r => r.Resource).ToList();
    }

    /// <inheritdoc/>
    public async Task<Conversation> CreateConversationAsync(string instanceId, ChatSessionProperties chatSessionProperties)
    {
        ArgumentException.ThrowIfNullOrEmpty(chatSessionProperties.Name);

        var newConversationId = Guid.NewGuid().ToString().ToLower();
        Conversation newConversation = new()
        {
            Id = newConversationId,
            SessionId = newConversationId,
            Name = newConversationId,
            DisplayName = chatSessionProperties.Name,
            Type = _sessionType,
            UPN = _userIdentity.UPN!
        };

        _ = await _conversationResourceProvider.UpsertResourceAsync<Conversation, ResourceProviderUpsertResult<Conversation>>(
            instanceId,
            newConversation,
            _userIdentity);

        return newConversation;
    }

    /// <inheritdoc/>
    public async Task<Conversation> RenameConversationAsync(string instanceId, string sessionId, ChatSessionProperties chatSessionProperties)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        ArgumentException.ThrowIfNullOrEmpty(chatSessionProperties.Name);

        var result = await _conversationResourceProvider.UpdateResourcePropertiesAsync<Conversation, ResourceProviderUpsertResult<Conversation>>(
            instanceId,
            sessionId,
            new Dictionary<string, object?>
                                             {
                { "/displayName", chatSessionProperties.Name }
            },
            _userIdentity);

        return result.Resource!;
    }

    /// <inheritdoc/>
    public async Task DeleteConversationAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        await _conversationResourceProvider.DeleteResourceAsync<Conversation>(
            instanceId,
            sessionId,
            _userIdentity);
    }

    #endregion

    #region Asynchronous completion operations

    /// <inheritdoc/>
    public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);
            var operationStartTime = DateTime.UtcNow;

            completionRequest = await PrepareCompletionRequest(completionRequest, true);

            var conversationItems = await CreateConversationItemsAsync(instanceId, completionRequest, _userIdentity);

            var agentOption = await ProcessGatekeeperOptions(instanceId, completionRequest);

            await _cosmosDBService.UpsertLongRunningOperationContextAsync(new LongRunningOperationContext
            {
                OperationId = completionRequest.OperationId!,
                AgentName = completionRequest.AgentName!,
                SessionId = completionRequest.SessionId!,
                UserMessageId = conversationItems.UserMessage.Id,
                AgentMessageId = conversationItems.AgentMessage.Id,
                CompletionPromptId = conversationItems.CompletionPrompt.Id,
                GatekeeperOverride = agentOption,
                StartTime = operationStartTime,
                UPN = _userIdentity.UPN!
            });

            // Start the completion operation.
            var result = await GetDownstreamAPIService(agentOption).StartCompletionOperation(instanceId, completionRequest);

            if (result.Status == OperationStatus.Failed)
            {
                // In case the completion operation fails to start properly, we need to update the user and agent messages accordingly.

                var patchOperations = new List<IPatchOperationItem>
                {
                    new PatchOperationItem<Message>
                    {
                        ItemId = conversationItems.UserMessage.Id,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed }
                        }
                    },
                    new PatchOperationItem<Message>
                    {
                        ItemId = conversationItems.AgentMessage.Id,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed },
                            { "/text", result.StatusMessage }
                        }
                    }
                };

                var patchedItems = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
                    completionRequest.SessionId!,
                    patchOperations
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting completion operation in conversation {SessionId}.",
                completionRequest.SessionId);

            // TODO: Depending on the type of failure, we should update the agent message to reflect the failure.

            return new LongRunningOperation
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                StatusMessage = "Could not start completion operation due to an internal error."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
    {
        try
        {
            var operationContext = await _cosmosDBService.GetLongRunningOperationContextAsync(operationId);
            var operationStatus = await GetDownstreamAPIService(operationContext.GatekeeperOverride).GetCompletionOperationStatus(instanceId, operationId);

            if ((DateTime.UtcNow - operationContext.StartTime).TotalMinutes > 30
                && (operationStatus.Status == OperationStatus.Pending || operationStatus.Status == OperationStatus.InProgress))
            {
                // We've hit the hard stop time for the operation.

                var patchOperations = new List<IPatchOperationItem>
                {
                    new PatchOperationItem<Message>
                    {
                        ItemId = operationContext.UserMessageId,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed },
                            { "/text", "The completion operation has exceeded the maximum time allowed." }
                        }
                    },
                    new PatchOperationItem<Message>
                    {
                        ItemId = operationContext.AgentMessageId,
                        PropertyValues = new Dictionary<string, object?>
                        {
                            { "/status", OperationStatus.Failed },
                            { "/text", "The completion operation has exceeded the maximum time allowed." },
                            { "/timeStamp", DateTime.UtcNow }
                        }
                    }
                };

                var patchedItems = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
                    operationContext.SessionId,
                    patchOperations
                );

                return new LongRunningOperation
                {
                    OperationId = operationId,
                    StatusMessage = "The completion operation has exceeded the maximum time allowed.",
                    Result = new Message
                    {
                        OperationId = operationId,
                        Status = OperationStatus.Failed,
                        Text = "The completion operation has exceeded the maximum time allowed.",
                        TimeStamp = DateTime.UtcNow,
                        SenderDisplayName = operationContext.AgentName
                    },
                    Status = OperationStatus.Failed
                };
            }

            if (operationStatus.Result is JsonElement jsonElement)
            {
                var completionResponse = jsonElement.Deserialize<CompletionResponse>();

                var agentMessage = await ProcessCompletionResponse(
                    operationContext,
                    completionResponse!,
                    operationStatus.Status);

                if (agentMessage.Content is { Count: > 0 })
                {
                    foreach (var content in agentMessage.Content)
                    {
                        content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                    }
                }

                operationStatus.Result = agentMessage;
                if (completionResponse != null)
                {
                    operationStatus.PromptTokens = completionResponse.PromptTokens;
                }

                return operationStatus;
            }

            operationStatus.Result = new Message
            {
                OperationId = operationId,
                Status = operationStatus.Status,
                Text = operationStatus.StatusMessage ?? "The completion operation is in progress.",
                TimeStamp = DateTime.UtcNow,
                SenderDisplayName = operationContext.AgentName
            };

            return operationStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving the status for the operation with id {OperationId}.",
                operationId);
            return new LongRunningOperation
            {
                OperationId = operationId,
                StatusMessage = "Could not retrieve the status of the operation due to an internal error.",
                Result = new Message
                {
                    OperationId = operationId,
                    Status = OperationStatus.Failed,
                    Text = "Could not retrieve the status of the operation due to an internal error.",
                    TimeStamp = DateTime.UtcNow
                },
                Status = OperationStatus.Failed
            };
        }
    }

    #endregion

    #region Synchronous completion operations

    /// <inheritdoc/>
    public async Task<Message> GetChatCompletionAsync(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(completionRequest.SessionId);
            var operationStartTime = DateTime.UtcNow;

            completionRequest = await PrepareCompletionRequest(completionRequest);

            var conversationItems = await CreateConversationItemsAsync(instanceId, completionRequest, _userIdentity);

            var agentOption = await ProcessGatekeeperOptions(instanceId, completionRequest);

            // Generate the completion to return to the user.
            var completionResponse = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, completionRequest);

            var agentMessage = await ProcessCompletionResponse(
                new LongRunningOperationContext
                {
                    AgentName = completionRequest.AgentName!,
                    SessionId = completionRequest.SessionId!,
                    OperationId = completionRequest.OperationId!,
                    UserMessageId = conversationItems.UserMessage.Id,
                    AgentMessageId = conversationItems.AgentMessage.Id,
                    CompletionPromptId = conversationItems.CompletionPrompt.Id,
                    StartTime = operationStartTime,
                    UPN = _userIdentity.UPN!
                },
                completionResponse,
                OperationStatus.Completed);

            return agentMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion in conversation {SessionId} for user prompt [{UserPrompt}].",
                completionRequest.SessionId, completionRequest.UserPrompt);
            return new Message
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = "Could not generate a completion due to an internal error."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<Message> GetCompletionAsync(string instanceId, CompletionRequest directCompletionRequest)
    {
        try
        {
            directCompletionRequest = await PrepareCompletionRequest(directCompletionRequest);

            var agentOption = await ProcessGatekeeperOptions(instanceId, directCompletionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, directCompletionRequest);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Completed,
                Text = result.Completion
                    ?? (result.Content?.Where(c => c.Type == MessageContentItemTypes.Text).FirstOrDefault() as OpenAITextMessageContentItem)?.Value
                        ?? "Could not generate a completion due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion for user prompt [{directCompletionRequest.UserPrompt}].");
            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = "Could not generate a completion due to an internal error."
            };
        }
    }

    #endregion

    #region Attachments

    /// <inheritdoc/>
    public async Task<ResourceProviderUpsertResult<AttachmentFile>> UploadAttachment(string instanceId, string sessionId, AttachmentFile attachmentFile, string agentName, UnifiedUserIdentity userIdentity)
    {
        var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(instanceId, agentName, userIdentity);
        var aiModelBase = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(agentBase.AIModelObjectId!, userIdentity);
        var apiEndpointConfiguration = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(aiModelBase.EndpointObjectId!, userIdentity);

        var agentRequiresOpenAIAssistants = agentBase.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants);

        attachmentFile.SecondaryProvider = agentRequiresOpenAIAssistants
            ? ResourceProviderNames.FoundationaLLM_AzureOpenAI
            : null;
        var attachmentUpsertResult = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile,
                _userIdentity);

        // TODO: Improve the logic of setting MustCreateAssistantFile to avoid unnecessary uploads to the Azure OpenAI file store.

        if (agentRequiresOpenAIAssistants)
        {
            var fileMapping = new AzureOpenAIFileMapping
            {
                Name = string.Empty,
                Id = string.Empty,
                UPN = userIdentity.UPN!,
                InstanceId = instanceId,
                FileObjectId = attachmentUpsertResult.ObjectId!,
                OriginalFileName = attachmentFile.OriginalFileName,
                FileContentType = attachmentFile.ContentType!,
                OpenAIEndpoint = apiEndpointConfiguration.Url,
                OpenAIFileId = string.Empty
            };

            var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
            {
                Parameters = new()
                    {
                        { AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId, agentBase.ObjectId! },
                        { AzureOpenAIResourceProviderUpsertParameterNames.ConversationId, sessionId },
                        { AzureOpenAIResourceProviderUpsertParameterNames.AttachmentObjectId, attachmentUpsertResult.ObjectId },
                        { AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIFile, true }
                    }
            };

            var extension = Path.GetExtension(attachmentFile.OriginalFileName).ToLowerInvariant().Replace(".", string.Empty);
            if (_azureOpenAIFileSearchFileExtensions.Contains(extension))
            {
                // The file also needs to be vectorized for the OpenAI assistant.
                fileMapping.FileRequiresVectorization = true;
            }

            var fileMappingUpsertResult = await _azureOpenAIResourceProvider.UpsertResourceAsync<AzureOpenAIFileMapping, ResourceProviderUpsertResult<AzureOpenAIFileMapping>>(
                instanceId,
                fileMapping,
                userIdentity,
                resourceProviderUpsertOptions);

            await _attachmentResourceProvider.UpdateResourcePropertiesAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile.Name!,
                new Dictionary<string, object?>
                {
                    { "/secondaryProviderObjectId", fileMappingUpsertResult.Resource!.OpenAIFileId }
                },
                userIdentity);
            attachmentUpsertResult.Resource!.SecondaryProviderObjectId = fileMappingUpsertResult.Resource!.OpenAIFileId;
        }

        return attachmentUpsertResult;
    }

    /// <inheritdoc/>
    public async Task<AttachmentFile?> DownloadAttachment(string instanceId, string fileProvider, string fileId, UnifiedUserIdentity userIdentity)
    {
        try
        {
            if (fileProvider == ResourceProviderNames.FoundationaLLM_AzureOpenAI)
            {
                var result = await _azureOpenAIResourceProvider.ExecuteResourceActionAsync<AzureOpenAIFileMapping, object?, ResourceProviderActionResult<FileContent>>(
                    instanceId,
                    fileId,
                    ResourceProviderActions.LoadFileContent,
                    null,
                    userIdentity);

                return new AttachmentFile
                {
                    Name = result.Resource!.Name,
                    OriginalFileName = result.Resource!.OriginalFileName,
                    ContentType = result.Resource!.ContentType,
                    Content = result.Resource!.BinaryContent!.Value.ToArray()
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading attachment {FileId} from {FileProvider}.", fileId, fileProvider);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<Dictionary<string, ResourceProviderDeleteResult?>> DeleteAttachments(
        string instanceId, List<string> resourcePaths, UnifiedUserIdentity userIdentity)
    {
        var results = resourcePaths.ToDictionary(key => key, value => (ResourceProviderDeleteResult?)null);

        foreach (var resourcePath in resourcePaths)
        {
            try
            {
                if (!ResourcePath.TryParseResourceProvider(resourcePath, out var resourceProviderName))
                    throw new ResourceProviderException(
                        $"Invalid resource provider for resource path [{resourcePath}].");

                if (resourceProviderName != ResourceProviderNames.FoundationaLLM_Attachment)
                    throw new ResourceProviderException(
                        $"The resource provider [{resourceProviderName}] is not supported by the delete attachments endpoint.");

                await _attachmentResourceProvider.HandleDeleteAsync(resourcePath, userIdentity);
                results[resourcePath] = new ResourceProviderDeleteResult()
                {
                    Deleted = true
                };
            }
            catch (ResourceProviderException rpex)
            {
                _logger.LogError(rpex, "{Message}", rpex.Message);

                results[resourcePath] = new ResourceProviderDeleteResult()
                {
                    Deleted = false,
                    Reason = rpex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error when handling the deletion for resource path [{ResourcePath}].", resourcePath);

                results[resourcePath] = new ResourceProviderDeleteResult()
                {
                    Deleted = false,
                    Reason = $"There was an error when handling the deletion for resource path [{resourcePath}]."
                };
            }
        }

        return results;
    }

    #endregion

    #region Conversation messages

    /// <inheritdoc/>
    public async Task<List<Message>> GetChatSessionMessagesAsync(string instanceId, string sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);
        var messages = await _cosmosDBService.GetSessionMessagesAsync(sessionId, _userIdentity.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));

        // Get a list of all attachment IDs in the messages.
        var attachmentIds = messages.SelectMany(m => m.Attachments ?? Enumerable.Empty<string>()).Distinct().ToList();
        if (attachmentIds.Count > 0)
        {
            var filter = new ResourceFilter
            {
                ObjectIDs = attachmentIds
            };
            // Get the attachment details from the attachment resource provider.
            var result = await _attachmentResourceProvider!.HandlePostAsync(
                $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Attachment}/{AttachmentResourceTypeNames.Attachments}/{ResourceProviderActions.Filter}",
                JsonSerializer.Serialize(filter),
                null,
                _userIdentity);
            //var list = result as IEnumerator<AttachmentFile>;
            var attachmentReferences = new List<AttachmentDetail>();

            attachmentReferences.AddRange(from attachment in (IEnumerable<AttachmentFile>)result select AttachmentDetail.FromAttachmentFile(attachment));

            if (attachmentReferences.Count > 0)
            {
                // Add the attachment details to the messages.
                foreach (var message in messages)
                {
                    if (message.Attachments is { Count: > 0 })
                    {
                        var messageAttachmentDetails = new List<AttachmentDetail>();
                        foreach (var attachment in message.Attachments)
                        {
                            var attachmentDetail = attachmentReferences.FirstOrDefault(ad => ad.ObjectId == attachment);
                            if (attachmentDetail != null)
                            {
                                messageAttachmentDetails.Add(attachmentDetail);
                            }
                        }
                        message.AttachmentDetails = messageAttachmentDetails;
                    }
                }
            }
        }

        foreach (var message in messages)
        {
            if (message.Content is { Count: > 0 })
            {
                foreach (var content in message.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }
        }

        return messages.ToList();
    }

    /// <inheritdoc/>
    public async Task RateMessageAsync(string instanceId, string id, string sessionId, MessageRatingRequest rating)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(sessionId);

        await _cosmosDBService.PatchSessionsItemPropertiesAsync<Message>(
            id,
            sessionId,
            new Dictionary<string, object?>
            {
                { "/rating", rating.Rating },
                { "/ratingComments", rating.Comments }
            });
    }

    /// <inheritdoc/>
    public async Task<CompletionPrompt> GetCompletionPrompt(string instanceId, string sessionId, string completionPromptId)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNullOrEmpty(completionPromptId);

        return await _cosmosDBService.GetCompletionPromptAsync(sessionId, completionPromptId);
    }

    #endregion

    #region Configuration

    /// <inheritdoc/>
    public async Task<CoreConfiguration> GetCoreConfiguration(string instanceId, UnifiedUserIdentity userIdentity)
    {
        var configuration = new CoreConfiguration
        {
            FileStoreConnectors = await GetFileStoreConnectors(instanceId, userIdentity),
            MaxUploadsPerMessage = await GetCoreConfigurationValue<int>(
                instanceId,
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_MaxUploadsPerMessage,
                userIdentity),
            CompletionResponsePollingIntervalSeconds = await GetCoreConfigurationValue<int>(
                instanceId,
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CompletionResponsePollingIntervalSeconds,
                userIdentity),
        };

        return configuration;
    }

    #endregion

    #region Private helper methods

    /// <inheritdoc/>
    private async Task<IEnumerable<APIEndpointConfiguration>> GetFileStoreConnectors(string instanceId, UnifiedUserIdentity userIdentity)
    {
        var apiEndpointConfigurations = await _configurationResourceProvider.GetResourcesAsync<APIEndpointConfiguration>(instanceId, userIdentity);
        var resources = apiEndpointConfigurations.Select(c => c.Resource).ToList();
        return resources.Where(c => c.Category == APIEndpointCategory.FileStoreConnector);
    }

    private IDownstreamAPIService GetDownstreamAPIService(AgentGatekeeperOverrideOption agentOption) =>
        ((agentOption == AgentGatekeeperOverrideOption.UseSystemOption) && _settings.BypassGatekeeper)
        || (agentOption == AgentGatekeeperOverrideOption.MustBypass)
            ? _orchestrationAPIService
            : _gatekeeperAPIService;

    private async Task<AgentGatekeeperOverrideOption> ProcessGatekeeperOptions(string instanceId, CompletionRequest completionRequest)
    {
        var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
            instanceId,
            completionRequest.AgentName!,
            _userIdentity);

        if (agentBase?.GatekeeperSettings?.UseSystemSetting == false)
        {
            // Agent does not want to use system settings, however it does not have any Gatekeeper options either
            // Consequently, a request to bypass Gatekeeper will be returned.
            if (agentBase!.GatekeeperSettings!.Options == null || agentBase.GatekeeperSettings.Options.Length == 0)
                return AgentGatekeeperOverrideOption.MustBypass;

            completionRequest.GatekeeperOptions = agentBase.GatekeeperSettings.Options;
            return AgentGatekeeperOverrideOption.MustCall;
        }

        return AgentGatekeeperOverrideOption.UseSystemOption;
    }

    /// <summary>
    /// Add session message
    /// </summary>
    private async Task<(Message UserMessage, Message AgentMessage, CompletionPrompt CompletionPrompt)> CreateConversationItemsAsync(string instanceId, CompletionRequest request, UnifiedUserIdentity userIdentity)
    {
        var userMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            Sender = nameof(Participants.User),
            Text = request.UserPrompt,
            UPN = userIdentity.UPN!,
            SenderDisplayName = userIdentity.Name,
            Attachments = request.Attachments,
            Status = OperationStatus.Pending,
            OperationId = request.OperationId
        };

        var agentMessage = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            Sender = nameof(Participants.Agent),
            UPN = userIdentity.UPN!,
            SenderDisplayName = request.AgentName,
            Status = OperationStatus.Pending,
            OperationId = request.OperationId
        };

        var completionPrompt = new CompletionPrompt
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = request.SessionId!,
            MessageId = agentMessage.Id,
            Prompt = string.Empty
        };

        agentMessage.CompletionPromptId = completionPrompt.Id;

        // Adds the incoming message to the session and updates the session with token usage.
        await _cosmosDBService.UpsertSessionBatchAsync(userMessage, agentMessage, completionPrompt);

        return (userMessage, agentMessage, completionPrompt);
    }

    private async Task<Message> ProcessCompletionResponse(
        LongRunningOperationContext operationContext,
        CompletionResponse completionResponse,
        OperationStatus operationStatus)
    {
        #region Process content

        var newContent = new List<MessageContent>();

        if (completionResponse.Content is { Count: > 0 })
        {
            foreach (var content in completionResponse.Content)
            {
                switch (content)
                {
                    case OpenAITextMessageContentItem textMessageContent:
                        if (textMessageContent.Annotations.Count > 0)
                        {
                            foreach (var annotation in textMessageContent.Annotations)
                            {
                                newContent.Add(new MessageContent
                                {
                                    Type = FileMethods.GetMessageContentFileType(annotation.Text, annotation.Type),
                                    FileName = annotation.Text,
                                    Value = annotation.FileUrl
                                });
                            }
                        }
                        newContent.Add(new MessageContent
                        {
                            Type = textMessageContent.Type,
                            Value = textMessageContent.Value
                        });
                        break;
                    case OpenAIImageFileMessageContentItem imageFileMessageContent:
                        newContent.Add(new MessageContent
                        {
                            Type = imageFileMessageContent.Type,
                            Value = imageFileMessageContent.FileUrl
                        });
                        break;
                }
            }
        }

        #endregion

        var completionPromptText =
            $"User prompt: {completionResponse.UserPrompt}{Environment.NewLine}Agent: {completionResponse.AgentName}{Environment.NewLine}Prompt template: {(!string.IsNullOrWhiteSpace(completionResponse.FullPrompt) ? completionResponse.FullPrompt : completionResponse.PromptTemplate)}";

        var patchOperations = new List<IPatchOperationItem>
        {
            // TODO: This update only needs to be done the first time a completion is processed (i.e. when status is retrieved).
            new PatchOperationItem<Message>
            {
                ItemId = operationContext.UserMessageId,
                PropertyValues = new Dictionary<string, object?>
                {
                    { "/tokens", completionResponse.PromptTokens },
                    { "/status", operationStatus }
                }
            },
            new PatchOperationItem<Message>
            {
                ItemId = operationContext.AgentMessageId,
                PropertyValues = new Dictionary<string, object?>
                {
                    { "/tokens", completionResponse.CompletionTokens },
                    { "/text", completionResponse.Completion },
                    { "/contentArtifacts", completionResponse.ContentArtifacts },
                    { "/content", newContent },
                    { "/analysisResults", completionResponse.AnalysisResults },
                    { "/status", operationStatus },
                    { "/timeStamp", DateTime.UtcNow }
                }
            },
            new PatchOperationItem<CompletionPrompt>
            {
                ItemId = operationContext.CompletionPromptId,
                PropertyValues = new Dictionary<string, object?> { { "/prompt", completionPromptText } }
            },
        };

        var patchedItems = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
            operationContext.SessionId,
            patchOperations
        );

        var agentMessage = patchedItems[operationContext.AgentMessageId] as Message;

        return agentMessage ?? new Message
        {
            OperationId = operationContext.OperationId,
            Status = OperationStatus.Failed,
            Text = "Could not retrieve the status of the operation due to an internal error."
        };
    }

    /// <summary>
    /// Pre-processing of incoming completion request.
    /// </summary>
    /// <param name="request">The completion request.</param>
    /// <param name="longRunningOperation">Indicates whether this is a long-running operation.</param>
    /// <returns>The updated completion request with pre-processing applied.</returns>
    private async Task<CompletionRequest> PrepareCompletionRequest(CompletionRequest request, bool longRunningOperation = false)
    {
        request.OperationId = Guid.NewGuid().ToString();
        request.LongRunningOperation = longRunningOperation;
        List<MessageHistoryItem> messageHistoryList = [];

        // Retrieve conversation, including latest prompt.
        var messages = await _cosmosDBService.GetSessionMessagesAsync(request.SessionId!, _userIdentity.UPN!);
        foreach (var message in messages)
        {
            var messageText = message.Text;
            if (message.Content is { Count: > 0 })
            {
                StringBuilder text = new();
                foreach (var content in message.Content.Where(content => content.Type == MessageContentItemTypes.Text))
                {
                    text.Append(content.Value);
                }
                messageText = text.ToString();
            }

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                messageHistoryList.Add(new MessageHistoryItem(message.Sender, messageText));
            }
        }

        request.MessageHistory = messageHistoryList;

        return request;
    }

    private async Task<T> GetCoreConfigurationValue<T>(string instanceId, string configurationName, UnifiedUserIdentity userIdentity)
    {
        var appConfigurationValue = await _configurationResourceProvider.GetResourceAsync<AppConfigurationKeyBase>(
            instanceId,
            configurationName,
            userIdentity);

        return ConfigurationValue<T>.Deserialize(appConfigurationValue.Value!).GetValueForUser(userIdentity.UPN!);
    }

    private static async Task<string> GetBaseUrl(
        IConfiguration configuration,
        IHttpClientFactoryService httpClientFactory,
        ICallContext callContext)
    {
        var baseUrl = configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!;
        try
        {
            var baseUrlOverride = await httpClientFactory.CreateClient<string?>(
                HttpClientNames.CoreAPI,
                callContext.CurrentUserIdentity!,
                BuildClient);
            if (!string.IsNullOrWhiteSpace(baseUrlOverride))
            {
                baseUrl = baseUrlOverride;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Ignore the exception since we should always fall back to the configured value.
        }

        return baseUrl;
    }

    private static string? BuildClient(Dictionary<string, object> parameters) =>
        parameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString();

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex ChatSessionNameReplacementRegex();

    private string? ResolveContentDeepLinks(string? text, string rootUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }
        const string token = "{{fllm_base_url}}";
        // If rootUrl ends with a slash, remove it.
        if (rootUrl.EndsWith('/'))
        {
            rootUrl = rootUrl[..^1];
        }
        return text.Replace(token, rootUrl);
    }

    private static UnifiedUserIdentity ValidateUserIdentity(UnifiedUserIdentity? userIdentity)
    {
        if (userIdentity == null)
            throw new InvalidOperationException("The call context does not contain a valid user identity.");

        if (string.IsNullOrWhiteSpace(userIdentity.UPN))
            throw new InvalidOperationException("The user identity provided by the call context does not contain a valid UPN.");

        return userIdentity;
    }

    #endregion
}
