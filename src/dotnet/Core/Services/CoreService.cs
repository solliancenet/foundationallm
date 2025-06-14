using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Constants.Orchestration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureAI;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Utils;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
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
/// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to build HTTP clients.</param>
/// <param name="contextServiceClient">The <see cref="IContextServiceClient"/> used to interact with the Context API.</param>
public partial class CoreService(
    IAzureCosmosDBService cosmosDBService,
    IEnumerable<IDownstreamAPIService> downstreamAPIServices,
    ILogger<CoreService> logger,
    IOptions<ClientBrandingConfiguration> brandingSettings,
    IOptions<CoreServiceSettings> settings,
    IOrchestrationContext callContext,
    IEnumerable<IResourceProviderService> resourceProviderServices,
    IConfiguration configuration,
    IHttpClientFactoryService httpClientFactory,
    IContextServiceClient contextServiceClient) : ICoreService
{
    private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
    private readonly IDownstreamAPIService _gatekeeperAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.GatekeeperAPI);
    private readonly IDownstreamAPIService _orchestrationAPIService = downstreamAPIServices.Single(das => das.APIName == HttpClientNames.OrchestrationAPI);
    private readonly ILogger<CoreService> _logger = logger;
    private readonly UnifiedUserIdentity _userIdentity = ValidateUserIdentity(callContext.CurrentUserIdentity);
    private readonly string _sessionType = brandingSettings.Value.KioskMode ? ConversationTypes.KioskSession : ConversationTypes.Session;
    private readonly CoreServiceSettings _settings = settings.Value;
    private readonly IContextServiceClient _contextServiceClient = contextServiceClient;

    private readonly string _baseUrl = GetBaseUrl(configuration, httpClientFactory, callContext).GetAwaiter().GetResult();

    private readonly IResourceProviderService _attachmentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
    private readonly IResourceProviderService _agentResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
    private readonly IResourceProviderService _azureAIResourceProvider =
        resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_AzureAI);
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

    private readonly HashSet<string> _azureAIAgentServiceFileSearchFileExtensions =
        settings.Value.AzureAIAgentsFileSearchFileExtensions
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

        var newConversationId = $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToBase64String()}";
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

            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                completionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new LongRunningOperation
                {
                    OperationId = completionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    StatusMessage = message
                };
            }

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, completionRequest.SessionId, _userIdentity);

            completionRequest = await PrepareCompletionRequest(instanceId, completionRequest, agentBase, true);

            var conversationItems = await CreateConversationItemsAsync(completionRequest, _userIdentity);

            var agentOption = GetGatekeeperOption(agentBase, completionRequest);

            var operationContext = new LongRunningOperationContext
            {
                InstanceId = instanceId,
                OperationId = completionRequest.OperationId!,
                AgentName = completionRequest.AgentName!,
                SessionId = completionRequest.SessionId!,
                UserMessageId = conversationItems.UserMessage.Id,
                AgentMessageId = conversationItems.AgentMessage.Id,
                CompletionPromptId = conversationItems.CompletionPrompt.Id,
                GatekeeperOverride = agentOption,
                SemanticCacheSettings = (agentBase.CacheSettings?.SemanticCacheEnabled ?? false)
                    ? agentBase.CacheSettings?.SemanticCacheSettings
                    : null,
                StartTime = operationStartTime,
                UPN = _userIdentity.UPN!
            };
            await _cosmosDBService.UpsertLongRunningOperationContextAsync(operationContext);

            // Start the completion operation.
            var result = await GetDownstreamAPIService(agentOption).StartCompletionOperation(instanceId, completionRequest);

            switch (result.Status)
            {
                case OperationStatus.Failed:
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

                        break;
                    }
                case OperationStatus.Completed:
                    {
                        // If the completion operation completes immediately, we need to process the completion response and update the user and agent messages accordingly.

                        var processedOperation = await ProcessLongRunningOperation(operationContext, result);
                        return processedOperation;
                    }
                default:
                    break;
            }

            return result;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error starting completion operation in conversation {SessionId}.",
                completionRequest.SessionId);

            return new LongRunningOperation
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                StatusMessage = rpex.StatusCode == StatusCodes.Status403Forbidden
                              ? "Could not start completion operation because access is forbidden."
                              : "Could not start completion operation due to an internal error."
            };
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

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, operationContext.SessionId, _userIdentity);

            var operation = await GetDownstreamAPIService(operationContext.GatekeeperOverride).GetCompletionOperationStatus(instanceId, operationId);

            var processedOperation = await ProcessLongRunningOperation(operationContext, operation);
            return processedOperation;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error retrieving the status for the operation with id {OperationId}.",
                operationId);

            return new LongRunningOperation
            {
                OperationId = operationId,
                Status = OperationStatus.Failed,
                StatusMessage = rpex.StatusCode == StatusCodes.Status403Forbidden
                              ? "Could not retrieve the status of the operation because access is forbidden."
                              : "Could not retrieve the status of the operation due to an internal error."
            };
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

    private async Task<LongRunningOperation> ProcessLongRunningOperation(
        LongRunningOperationContext operationContext,
        LongRunningOperation operation)
    {
        if ((DateTime.UtcNow - operationContext.StartTime).TotalMinutes > 30
                && (operation.Status == OperationStatus.Pending || operation.Status == OperationStatus.InProgress))
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

            _ = await _cosmosDBService.PatchMultipleSessionsItemsInTransactionAsync(
                operationContext.SessionId,
                patchOperations
            );

            return new LongRunningOperation
            {
                OperationId = operation.OperationId,
                StatusMessage = "The completion operation has exceeded the maximum time allowed.",
                Result = new Message
                {
                    OperationId = operation.OperationId,
                    Status = OperationStatus.Failed,
                    Text = "The completion operation has exceeded the maximum time allowed.",
                    TimeStamp = DateTime.UtcNow,
                    SenderDisplayName = operationContext.AgentName
                },
                Status = OperationStatus.Failed
            };
        }

        if (operation.Result is JsonElement jsonElement)
        {
            var completionResponse = jsonElement.Deserialize<CompletionResponse>();

            var agentMessage = await ProcessCompletionResponse(
                operationContext,
                completionResponse!,
                operation.Status);

            if (agentMessage.Content is { Count: > 0 })
            {
                foreach (var content in agentMessage.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }

            operation.Result = agentMessage;
            if (completionResponse != null)
            {
                operation.PromptTokens = completionResponse.PromptTokens;
            }

            return operation;
        }

        operation.Result = new Message
        {
            OperationId = operation.OperationId,
            Status = operation.Status,
            Text = operation.StatusMessage ?? "The completion operation is in progress.",
            TimeStamp = DateTime.UtcNow,
            SenderDisplayName = operationContext.AgentName
        };

        return operation;
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

            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                completionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new Message
                {
                    OperationId = completionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    Text = message
                };
            }

            _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, completionRequest.SessionId, _userIdentity);

            completionRequest = await PrepareCompletionRequest(instanceId, completionRequest, agentBase);

            var conversationItems = await CreateConversationItemsAsync(completionRequest, _userIdentity);

            var agentOption = GetGatekeeperOption(agentBase, completionRequest);

            // Generate the completion to return to the user.
            var completionResponse = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, completionRequest);

            var agentMessage = await ProcessCompletionResponse(
                new LongRunningOperationContext
                {
                    InstanceId = instanceId,
                    AgentName = completionRequest.AgentName!,
                    SessionId = completionRequest.SessionId!,
                    OperationId = completionRequest.OperationId!,
                    UserMessageId = conversationItems.UserMessage.Id,
                    AgentMessageId = conversationItems.AgentMessage.Id,
                    CompletionPromptId = conversationItems.CompletionPrompt.Id,
                    GatekeeperOverride = agentOption,
                    SemanticCacheSettings = (agentBase.CacheSettings?.SemanticCacheEnabled ?? false)
                        ? agentBase.CacheSettings?.SemanticCacheSettings
                        : null,
                    StartTime = operationStartTime,
                    UPN = _userIdentity.UPN!
                },
                completionResponse,
                OperationStatus.Completed);

            if (agentMessage.Content is { Count: > 0 })
            {
                foreach (var content in agentMessage.Content)
                {
                    content.Value = ResolveContentDeepLinks(content.Value, _baseUrl);
                }
            }

            return agentMessage;
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, "Error getting completion in conversation {SessionId} for user prompt [{UserPrompt}].",
                 completionRequest.SessionId, completionRequest.UserPrompt);

            return new Message
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = rpex.StatusCode == StatusCodes.Status403Forbidden
                     ? "Could not generate a completion because access is forbidden."
                     : "Could not generate a completion due to an internal error."
            };
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
            var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                instanceId,
                directCompletionRequest.AgentName!,
                _userIdentity);

            if (AgentExpired(agentBase, out var message))
            {
                return new Message
                {
                    OperationId = directCompletionRequest.OperationId,
                    Status = OperationStatus.Failed,
                    Text = message
                };
            }

            directCompletionRequest = await PrepareCompletionRequest(instanceId, directCompletionRequest, agentBase);

            var agentOption = GetGatekeeperOption(agentBase, directCompletionRequest);

            // Generate the completion to return to the user.
            var result = await GetDownstreamAPIService(agentOption).GetCompletion(instanceId, directCompletionRequest);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Completed,
                Text = result.Completion
                    ?? (result.Content?.Where(c => c.Type == MessageContentItemTypes.Text).FirstOrDefault() as TextMessageContentItem)?.Value
                        ?? "Could not generate a completion due to an internal error."
            };
        }
        catch (ResourceProviderException rpex)
        {
            _logger.LogError(rpex, $"Error getting completion for user prompt [{{UserPrompt}}].", directCompletionRequest.UserPrompt);

            return new Message
            {
                OperationId = directCompletionRequest.OperationId,
                Status = OperationStatus.Failed,
                Text = rpex.StatusCode == StatusCodes.Status403Forbidden
                     ? "Could not generate a completion because access is forbidden."
                     : "Could not generate a completion due to an internal error."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting completion for user prompt [{{UserPrompt}}].", directCompletionRequest.UserPrompt);

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
    public async Task<ResourceProviderUpsertResult<AttachmentFile>> UploadAttachment(
        string instanceId, string sessionId, AttachmentFile attachmentFile, string agentName)
    {
        var agentBase = await _agentResourceProvider.GetResourceAsync<AgentBase>(instanceId, agentName, _userIdentity);

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        var agentRequiresOpenAIAssistants = agentBase.HasAzureOpenAIAssistantsWorkflow();
        var agentRequiresAzureAIAgentService = agentBase.HasAzureAIAgentServiceWorkflow();

        if (agentRequiresOpenAIAssistants)
        {
            var aiModelBase = await _aiModelResourceProvider.GetResourceAsync<AIModelBase>(agentBase.Workflow!.MainAIModelObjectId!, _userIdentity);

            attachmentFile.SecondaryProvider = agentRequiresOpenAIAssistants ? ResourceProviderNames.FoundationaLLM_AzureOpenAI : ResourceProviderNames.FoundationaLLM_AzureAI;

            var attachmentUpsertResult = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile,
                _userIdentity);

            var apiEndpointConfiguration = await _configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(aiModelBase.EndpointObjectId!, _userIdentity);

            var fileMapping = new AzureOpenAIFileMapping
            {
                Name = string.Empty,
                Id = string.Empty,
                UPN = _userIdentity.UPN!,
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
                _userIdentity,
                resourceProviderUpsertOptions);

            var newFileId = fileMappingUpsertResult.Resource!.OpenAIFileId;            
            
            await _attachmentResourceProvider.UpdateResourcePropertiesAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile.Name!,
                new Dictionary<string, object?>
                {
                    { "/secondaryProviderObjectId", newFileId }
                },
                _userIdentity);
            attachmentUpsertResult.Resource!.SecondaryProviderObjectId = newFileId;

            return attachmentUpsertResult;
        }

        if (agentRequiresAzureAIAgentService)
        {
            attachmentFile.SecondaryProvider = ResourceProviderNames.FoundationaLLM_AzureAI;
            var attachmentUpsertResult = await _attachmentResourceProvider.UpsertResourceAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile,
                _userIdentity);

            var workflow = agentBase.Workflow as AzureAIAgentServiceAgentWorkflow;
            var fileMapping = new AzureAIAgentFileMapping
            {
                Name = string.Empty,
                Id = string.Empty,
                UPN = _userIdentity.UPN!,
                InstanceId = instanceId,
                FileObjectId = attachmentUpsertResult.ObjectId!,
                OriginalFileName = attachmentFile.OriginalFileName,
                FileContentType = attachmentFile.ContentType!,
                ProjectConnectionString = workflow!.ProjectConnectionString!,
                AzureAIAgentFileId = string.Empty
            };

            var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
            {
                Parameters = new()
                {
                    { AzureAIResourceProviderUpsertParameterNames.AgentObjectId, agentBase.ObjectId! },
                    { AzureAIResourceProviderUpsertParameterNames.ConversationId, sessionId },
                    { AzureAIResourceProviderUpsertParameterNames.AttachmentObjectId, attachmentUpsertResult.ObjectId },
                    { AzureAIResourceProviderUpsertParameterNames.MustCreateAzureAIAgentFile, true }
                }
            };

            var extension = Path.GetExtension(attachmentFile.OriginalFileName).ToLowerInvariant().Replace(".", string.Empty);
            if (_azureAIAgentServiceFileSearchFileExtensions.Contains(extension))
            {
                // The file also needs to be vectorized for the Azure AI Agent Service agent.
                fileMapping.FileRequiresVectorization = true;
            }

            var fileMappingUpsertResult = await _azureAIResourceProvider.UpsertResourceAsync<AzureAIAgentFileMapping, ResourceProviderUpsertResult<AzureAIAgentFileMapping>>(
                instanceId,
                fileMapping,
                _userIdentity,
                resourceProviderUpsertOptions);

            var newFileId = fileMappingUpsertResult.Resource!.AzureAIAgentFileId;
            

            await _attachmentResourceProvider.UpdateResourcePropertiesAsync<AttachmentFile, ResourceProviderUpsertResult<AttachmentFile>>(
                instanceId,
                attachmentFile.Name!,
                new Dictionary<string, object?>
                {
                    { "/secondaryProviderObjectId", newFileId }
                },
                _userIdentity);
            attachmentUpsertResult.Resource!.SecondaryProviderObjectId = newFileId;

            return attachmentUpsertResult;
        }
       
        var serviceResult = await _contextServiceClient.CreateFile(
            instanceId,
            sessionId,
            attachmentFile.OriginalFileName,
            attachmentFile.ContentType!,
            new MemoryStream(attachmentFile.Content!));

            if (serviceResult.Success)
            {
                return new ResourceProviderUpsertResult<AttachmentFile>
                {
                    ObjectId = serviceResult.Result!.Id,
                    ResourceExists = false,
                    Resource = new AttachmentFile
                    {
                        Name = serviceResult.Result.Id,
                        ObjectId = serviceResult.Result.Id,
                        DisplayName = serviceResult.Result.FileName,
                        CreatedBy = serviceResult.Result.UPN,
                        CreatedOn = serviceResult.Result.CreatedAt,

                        ContentType = serviceResult.Result.ContentType,
                        Path = serviceResult.Result.FilePath,
                        OriginalFileName = serviceResult.Result.FileName,
                    }
                };
            }
            else
                throw new CoreServiceException(
                    serviceResult.ErrorMessage);
        
    }

    /// <inheritdoc/>
    public async Task<AttachmentFile?> DownloadAttachment(string instanceId, string fileProvider, string fileId)
    {
        try
        {
            switch (fileProvider)
            {

                case ResourceProviderNames.FoundationaLLM_AzureOpenAI:

                    var result = await _azureOpenAIResourceProvider.ExecuteResourceActionAsync<AzureOpenAIFileMapping, object?, ResourceProviderActionResult<FileContent>>(
                        instanceId,
                        fileId,
                        ResourceProviderActions.LoadFileContent,
                        null,
                        _userIdentity);

                    return new AttachmentFile
                    {
                        Name = result.Resource!.Name,
                        OriginalFileName = result.Resource!.OriginalFileName,
                        ContentType = result.Resource!.ContentType,
                        Content = result.Resource!.BinaryContent!.Value.ToArray()
                    };
                case ResourceProviderNames.FoundationaLLM_AzureAI:
                    var azureAIResponse = await _azureAIResourceProvider.ExecuteResourceActionAsync<AzureAIAgentFileMapping, object?, ResourceProviderActionResult<FileContent>>(
                        instanceId,
                        fileId,
                        ResourceProviderActions.LoadFileContent,
                        null,
                        _userIdentity);

                    return new AttachmentFile
                    {
                        Name = azureAIResponse.Resource!.Name,
                        OriginalFileName = azureAIResponse.Resource!.OriginalFileName,
                        ContentType = azureAIResponse.Resource!.ContentType,
                        Content = azureAIResponse.Resource!.BinaryContent!.Value.ToArray()
                    };
                case ContextProviderNames.FoundationaLLM_ContextAPI:

                    var responseMessage = await _contextServiceClient.GetFileContent(instanceId, fileId);

                    if (responseMessage.Success)
                    {
                        var content = new MemoryStream();
                        await responseMessage.Result!.FileContent!.CopyToAsync(content);

                        return new AttachmentFile
                        {
                            Name = responseMessage.Result!.FileName,
                            OriginalFileName = responseMessage.Result!.FileName,
                            ContentType = responseMessage.Result!.ContentType,
                            Content = content.ToArray()
                        };
                    }

                    break;
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
        string instanceId, List<string> resourcePaths)
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

                await _attachmentResourceProvider.HandleDeleteAsync(resourcePath, _userIdentity);
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

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        var messages = await _cosmosDBService.GetSessionMessagesAsync(sessionId, _userIdentity.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat messages."));

        // Get a list of all attachment IDs in the messages.
        var attachmentIds = messages.SelectMany(m => m.Attachments ?? Enumerable.Empty<string>()).Distinct().ToList();
        if (attachmentIds.Count > 0)
        {
            var contextAttachmentIds = new List<string>();
            var legacyAttachmentIds = new List<string>();
            var attachmentReferences = new List<AttachmentDetail>();

            foreach (var attachmentObjectId in attachmentIds)
            {
                if (ResourcePath.TryParseResourceProvider(attachmentObjectId, out string? resourceProvider))
                {
                    legacyAttachmentIds.Add(attachmentObjectId);
                }
                else
                {
                    contextAttachmentIds.Add(attachmentObjectId);
                }
            }

            if (legacyAttachmentIds.Count > 0)
            {
                var filter = new ResourceFilter
                {
                    ObjectIDs = legacyAttachmentIds
                };
                // Get the attachment details from the attachment resource provider.
                var result = await _attachmentResourceProvider!.HandlePostAsync(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Attachment}/{AttachmentResourceTypeNames.Attachments}/{ResourceProviderActions.Filter}",
                    JsonSerializer.Serialize(filter),
                    null,
                    _userIdentity);
                attachmentReferences.AddRange(from attachment in (IEnumerable<AttachmentFile>)result select AttachmentDetail.FromAttachmentFile(attachment));
            }
            else
            {
                var contextAttachmentResult =
                    contextAttachmentIds
                    .ToAsyncEnumerable()
                    .SelectAwait(async x => await _contextServiceClient.GetFileRecord(instanceId, x));
                await foreach (var attachment in contextAttachmentResult)
                {
                    attachmentReferences.Add(AttachmentDetail.FromContextFileRecord(attachment.Result!));
                }
            }

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

        return [.. messages];
    }

    /// <inheritdoc/>
    public async Task RateMessageAsync(string instanceId, string id, string sessionId, MessageRatingRequest rating)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

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

        _ = await _conversationResourceProvider.GetResourceAsync<Conversation>(instanceId, sessionId, _userIdentity);

        return await _cosmosDBService.GetCompletionPromptAsync(sessionId, completionPromptId);
    }

    #endregion

    #region Configuration

    /// <inheritdoc/>
    public async Task<CoreConfiguration> GetCoreConfiguration(string instanceId)
    {
        var configuration = new CoreConfiguration
        {
            FileStoreConnectors = await GetFileStoreConnectors(instanceId, _userIdentity),
            MaxUploadsPerMessage = await GetCoreConfigurationValue<int>(
                instanceId,
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_MaxUploadsPerMessage,
                _userIdentity),
            CompletionResponsePollingIntervalMilliseconds = await GetCoreConfigurationValue<int>(
                instanceId,
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CompletionResponsePollingIntervalMilliseconds,
                _userIdentity),
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

    private static AgentGatekeeperOverrideOption GetGatekeeperOption(
        AgentBase agent, CompletionRequest completionRequest)
    {
        if (agent.GatekeeperSettings?.UseSystemSetting == false)
        {
            // Agent does not want to use system settings, however it does not have any Gatekeeper options either
            // Consequently, a request to bypass Gatekeeper will be returned.
            if (agent.GatekeeperSettings!.Options == null || agent.GatekeeperSettings.Options.Length == 0)
                return AgentGatekeeperOverrideOption.MustBypass;

            completionRequest.GatekeeperOptions = agent.GatekeeperSettings.Options;
            return AgentGatekeeperOverrideOption.MustCall;
        }

        return AgentGatekeeperOverrideOption.UseSystemOption;
    }

    /// <summary>
    /// Add session message
    /// </summary>
    private async Task<(Message UserMessage, Message AgentMessage, CompletionPrompt CompletionPrompt)> CreateConversationItemsAsync(
        CompletionRequest request, UnifiedUserIdentity userIdentity)
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
        LongRunningOperationContext operationContext, CompletionResponse completionResponse, OperationStatus operationStatus)
    {
        #region Process content

        var newContent = new List<MessageContent>();

        if (completionResponse.Content is { Count: > 0 })
        {
            foreach (var content in completionResponse.Content)
            {
                switch (content)
                {
                    case TextMessageContentItem textMessageContent:
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
                    case ImageFileMessageContentItem imageFileMessageContent:
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
                    { "/status", operationStatus },
                    { "/textRewrite", completionResponse.UserPromptRewrite }
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
    /// <param name="instanceId">The FoundationaLLM instance ID.</param>
    /// <param name="request">The completion request.</param>
    /// <param name="agent">The <see cref="AgentBase"/> resource object.</param>
    /// <param name="longRunningOperation">Indicates whether this is a long-running operation.</param>
    /// <returns>The updated completion request with pre-processing applied.</returns>
    private async Task<CompletionRequest> PrepareCompletionRequest(
        string instanceId, CompletionRequest request, AgentBase agent, bool longRunningOperation = false)
    {
        request.LongRunningOperation = longRunningOperation;

        List<MessageHistoryItem> messageHistoryList = [];
        List<string> contentArtifactTypes = (agent.ConversationHistorySettings?.Enabled ?? false)
            ? [.. (agent.ConversationHistorySettings.HistoryContentArtifactTypes ?? string.Empty).Split(",", StringSplitOptions.RemoveEmptyEntries)]
            : [];

        // Retrieve the complete conversation.
        var messages = await _cosmosDBService.GetSessionMessagesAsync(request.SessionId!, _userIdentity.UPN!);
        var fileHistory = new List<FileHistoryItem>();
        int attachmentOrder = 0;
        foreach (var message in messages)
        {
            var messageText = message.Text;
            var messageTextRewrite = message.TextRewrite;
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
                var messageHistoryItem = new MessageHistoryItem(message.Sender, messageText, messageTextRewrite)
                {
                    ContentArtifacts = message.ContentArtifacts?.Where(ca => contentArtifactTypes.Contains(ca.Type ?? string.Empty)).ToList()
                };
                if (message.Attachments is { Count: > 0 })
                {
                    foreach (var attachmentObjectId in message.Attachments)
                    {
                        if (ResourcePath.TryParseResourceProvider(attachmentObjectId, out var resourceProviderName))
                        {
                            //Get resource path for attachment
                            var rp = ResourcePath.GetResourcePath(attachmentObjectId);
                            var file = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(instanceId, rp.MainResourceId!, _userIdentity);
                            fileHistory.Add(FileHistoryItem.FromAttachmentFile(file, ++attachmentOrder, false));
                        }
                        else
                        {
                            var fileResponse = await _contextServiceClient.GetFileRecord(instanceId, attachmentObjectId);
                            if (fileResponse.Success)
                            {
                                fileHistory.Add(FileHistoryItem.FromContextFileRecord(
                                    fileResponse.Result!,
                                    ++attachmentOrder,
                                    false,
                                    false));
                            }
                            else
                            {
                                _logger.LogError("Failed to retrieve file record for attachment {AttachmentObjectId}.", attachmentObjectId);
                            }
                        }
                    }
                }
                messageHistoryList.Add(messageHistoryItem);
            }
        }

        // If there is an attachment in the current message, add it to the file history.
        if (request.Attachments is { Count: > 0 })
        {
            foreach (var attachmentObjectId in request.Attachments)
            {
                if (ResourcePath.TryParseResourceProvider(attachmentObjectId, out var resourceProviderName))
                {
                    //Get resource path for attachment
                    var rp = ResourcePath.GetResourcePath(attachmentObjectId);
                    var file = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(instanceId, rp.MainResourceId!, _userIdentity);
                    fileHistory.Add(FileHistoryItem.FromAttachmentFile(file, ++attachmentOrder, true));                   
                }
                else
                {
                    var fileResponse = await _contextServiceClient.GetFileRecord(instanceId, attachmentObjectId);
                    if (fileResponse.Success)
                    {
                        fileHistory.Add(FileHistoryItem.FromContextFileRecord(
                            fileResponse.Result!,
                            ++attachmentOrder,
                            true,
                            true));
                    }
                    else
                    {
                        _logger.LogError("Failed to retrieve file record for attachment {AttachmentObjectId}.", attachmentObjectId);
                    }
                }
            }
        }

        // Include conversation file history regardless of the conversation history settings.
        if (fileHistory.Count > 0)
        {
            request.FileHistory = fileHistory;
        }

        // Only include message history if the conversation history settings are enabled.
        if (string.IsNullOrWhiteSpace(request.SessionId) ||
            !(agent.ConversationHistorySettings?.Enabled ?? false))
            return request;

        // Truncate the message history based on configuration.
        var max = agent.ConversationHistorySettings?.MaxHistory * 2 ?? null;
        if (max.HasValue && messageHistoryList.Count > max)
        {
            // Remove messages from the beginning of the list.
            messageHistoryList.RemoveRange(0, messageHistoryList.Count - max.Value);
        }

        request.MessageHistory = messageHistoryList;
        return request;
    }

    private bool AgentExpired(AgentBase agentBase, out string message)
    {
        // Check if the agent is expired.
        if (agentBase.ExpirationDate != null && agentBase.ExpirationDate < DateTime.UtcNow)
        {
            _logger.LogWarning("User has attempted to access an expired agent: {AgentName}.",
                agentBase.Name);
            {
                message = "Could not complete your request because the agent has expired.";
                return true;
            }
        }
        message = string.Empty;
        return false;
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
        IOrchestrationContext callContext)
    {
        var baseUrl = configuration[AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl]!;
        try
        {
            if (callContext.CurrentUserIdentity is { AssociatedWithAccessToken: false })
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

    private static string? ResolveContentDeepLinks(string? text, string rootUrl)
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
