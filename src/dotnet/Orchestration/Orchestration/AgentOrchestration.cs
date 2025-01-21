using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.Orchestration.Response.OpenAI;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Knowledge Management orchestration.
    /// </summary>
    /// <remarks>
    /// Constructor for default agent.
    /// </remarks>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="agentObjectId">The FoundationaLLM object identifier of the agent.</param>
    /// <param name="agent">The <see cref="KnowledgeManagementAgent"/> agent.</param>
    /// <param name="agentWorkflowMainAIModelAPIEndpoint">The URL of the API endpoint of the main AI model used by the agent workflow.</param>
    /// <param name="explodedObjects">A dictionary of objects retrieved from various object ids related to the agent. For more details see <see cref="LLMCompletionRequest.Objects"/> .</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="orchestrationService"></param>
    /// <param name="userPromptRewriteService">The <see cref="IUserPromptRewriteService"/> used to rewrite user prompts.</param>
    /// <param name="semanticCacheService">The <see cref="ISemanticCacheService"/> used to cache and retrieve completion responses.</param>
    /// <param name="logger">The logger used for logging.</param>
    /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/> used to create HttpClient instances.</param>
    /// <param name="resourceProviderServices">The dictionary of <see cref="IResourceProviderService"/></param>
    /// <param name="dataSourceAccessDenied">Inidicates that access was denied to all underlying data sources.</param>
    /// <param name="openAIVectorStoreId">The OpenAI Assistants vector store id.</param>
    /// <param name="longRunningOperationContext">The <see cref="LongRunningOperationContext"/> providing the context of the long-running operation.</param>
    /// <param name="completionRequestObserver">An optional observer for completion requests.</param>
    public class AgentOrchestration(
        string instanceId,
        string agentObjectId,
        KnowledgeManagementAgent? agent,
        string agentWorkflowMainAIModelAPIEndpoint,
        Dictionary<string, object>? explodedObjects,
        ICallContext callContext,
        ILLMOrchestrationService orchestrationService,
        IUserPromptRewriteService userPromptRewriteService,
        ISemanticCacheService semanticCacheService,
        ILogger<OrchestrationBase> logger,
        IHttpClientFactoryService httpClientFactoryService,
        Dictionary<string, IResourceProviderService> resourceProviderServices,
        bool? dataSourceAccessDenied,
        string? openAIVectorStoreId,
        LongRunningOperationContext? longRunningOperationContext,
        Func<LLMCompletionRequest, Task>? completionRequestObserver = null) : OrchestrationBase(orchestrationService)
    {
        private readonly string _instanceId = instanceId;
        private readonly string _agentObjectId = agentObjectId;
        private readonly KnowledgeManagementAgent? _agent = agent;
        private readonly string _agentWorkflowMainAIModelAPIEndpoint = agentWorkflowMainAIModelAPIEndpoint;
        private readonly Dictionary<string, object>? _explodedObjects = explodedObjects;
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<OrchestrationBase> _logger = logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly bool? _dataSourceAccessDenied = dataSourceAccessDenied;
        private readonly LongRunningOperationContext? _longRunningOperationContext = longRunningOperationContext;
        private readonly Func<LLMCompletionRequest, Task>? _completionRequestObserver = completionRequestObserver;

        private readonly IResourceProviderService _attachmentResourceProvider =
            resourceProviderServices[ResourceProviderNames.FoundationaLLM_Attachment];
        private readonly IResourceProviderService _azureOpenAIResourceProvider =
            resourceProviderServices[ResourceProviderNames.FoundationaLLM_AzureOpenAI];
        private readonly string? _openAIVectorStoreId = openAIVectorStoreId;
        private GatewayServiceClient _gatewayClient;

        private readonly IUserPromptRewriteService _userPromptRewriteService = userPromptRewriteService;
        private readonly ISemanticCacheService _semanticCacheService = semanticCacheService;

        /// <inheritdoc/>
        public override async Task<LongRunningOperation> StartCompletionOperation(CompletionRequest completionRequest)
        {
            var validationResponse = await ValidateCompletionRequest(completionRequest);
            if (validationResponse != null)
                return new LongRunningOperation
                {
                    OperationId = completionRequest.OperationId!,
                    Status = OperationStatus.Completed,
                    Result = validationResponse
                };

            if (_agent!.CacheSettings != null
                && _agent!.CacheSettings.SemanticCacheEnabled)
            {
                await HandlePromptRewrite(completionRequest);
                var cachedResponse = await GetCompletionResponseFromCache(completionRequest);
                if (cachedResponse != null)
                {
                    // Rewrite the operation id to match the completion request.
                    cachedResponse.OperationId = completionRequest.OperationId!;

                    return new LongRunningOperation
                    {
                        OperationId = completionRequest.OperationId!,
                        Status = OperationStatus.Completed,
                        Result = cachedResponse
                    };
                }
            }

            var llmCompletionRequest = await GetLLMCompletionRequest(completionRequest);
            if (_completionRequestObserver != null)
                await _completionRequestObserver(llmCompletionRequest);

            var result = await _orchestrationService.StartCompletionOperation(
                _instanceId,
                llmCompletionRequest);

            return result;
        }

        /// <inheritdoc/>
        public override async Task<LongRunningOperation> GetCompletionOperationStatus(string operationId)
        {
            var operationStatus = await _orchestrationService.GetCompletionOperationStatus(_instanceId, operationId);

            // Determine if the request is an end-state. If status is Failed, the error is located in the LLMCompletionResponse.Errors property.
            if (operationStatus.Status == OperationStatus.Completed || operationStatus.Status == OperationStatus.Failed)
            {
                // parse the LLM Completion response from JsonElement
                if (operationStatus.Result is JsonElement jsonElement)
                {
                    var llmCompletionResponse = JsonSerializer.Deserialize<LLMCompletionResponse>(jsonElement.ToString());
                    if (llmCompletionResponse != null)
                    {
                        var completionResponse = await GetCompletionResponse(operationId, llmCompletionResponse);

                        if ((_longRunningOperationContext?.SemanticCacheSettings != null)
                            && (
                                completionResponse.Errors == null
                                || completionResponse.Errors.Length == 0
                            ))
                        {
                            // This is a valid response that can be cached.
                            await SetCompletionResponseInCache(
                                _instanceId,
                                _longRunningOperationContext.AgentName,
                                completionResponse);
                        }

                        operationStatus.Result = completionResponse;
                    }
                }               
            }

            return operationStatus;
        }

        /// <inheritdoc/>
        public override async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            var validationResponse = await ValidateCompletionRequest(completionRequest);
            if (validationResponse != null)
                return validationResponse;

            if (_agent!.CacheSettings != null
                && _agent!.CacheSettings.SemanticCacheEnabled)
            {
                await HandlePromptRewrite(completionRequest);
                var cachedResponse = await GetCompletionResponseFromCache(completionRequest);
                if (cachedResponse != null)
                    return cachedResponse;
            }

            var llmCompletionRequest = await GetLLMCompletionRequest(completionRequest);
            if (_completionRequestObserver != null)
                await _completionRequestObserver(llmCompletionRequest);

            var llmCompletionResponse = await _orchestrationService.GetCompletion(
                _instanceId,
                llmCompletionRequest);

            var completionResponse = await GetCompletionResponse(completionRequest.OperationId!, llmCompletionResponse);

            if (_agent!.CacheSettings != null
                && _agent!.CacheSettings.SemanticCacheEnabled
                && (
                    completionResponse.Errors == null
                    || completionResponse.Errors.Length == 0
                ))
            {
                // This is a valid response that can be cached.
                await SetCompletionResponseInCache(
                    _instanceId,
                    _agent!.Name,
                    completionResponse);
            }

            return completionResponse;
        }

        private async Task<CompletionResponse?> ValidateCompletionRequest(CompletionRequest completionRequest)
        {
            _gatewayClient = new GatewayServiceClient(
                await _httpClientFactoryService
                    .CreateClient(HttpClientNames.GatewayAPI, _callContext.CurrentUserIdentity!),
                _logger);

            if (_dataSourceAccessDenied.HasValue
                && _dataSourceAccessDenied.Value)
                return new CompletionResponse
                {
                    OperationId = completionRequest.OperationId!,
                    Completion = "I have no knowledge that can be used to answer this question.",
                    UserPrompt = completionRequest.UserPrompt!,
                    AgentName = _agent!.Name
                };

            if (_agent!.ExpirationDate.HasValue && _agent.ExpirationDate.Value < DateTime.UtcNow)
                return new CompletionResponse
                {
                    OperationId = completionRequest.OperationId!,
                    Completion = $"The requested agent, {_agent.Name}, has expired and is unable to respond.",
                    UserPrompt = completionRequest.UserPrompt!,
                    AgentName = _agent.Name
                };

            return null;
        }

        private async Task HandlePromptRewrite(CompletionRequest completionRequest)
        {
            if (_agent!.TextRewriteSettings != null
                && _agent!.TextRewriteSettings.UserPromptRewriteEnabled)
            {
                if (!_userPromptRewriteService.HasUserPromptRewriterForAgent(_instanceId, _agent.Name))
                    await _userPromptRewriteService.InitializeUserPromptRewriterForAgent(
                        _instanceId,
                        _agent.Name,
                        _agent.TextRewriteSettings.UserPromptRewriteSettings!);

                await _userPromptRewriteService.RewriteUserPrompt(_instanceId, _agent.Name, completionRequest);
            }
        }

        private async Task<CompletionResponse?> GetCompletionResponseFromCache(CompletionRequest completionRequest)
        {
            if (!_semanticCacheService.HasCacheForAgent(_instanceId, _agent!.Name))
                await _semanticCacheService.InitializeCacheForAgent(
                    _instanceId,
                    _agent.Name,
                    _agent.CacheSettings!.SemanticCacheSettings!);

            var cachedResponse = await _semanticCacheService.GetCompletionResponseFromCache(
                _instanceId,
                _agent.Name,
                completionRequest);

            if (cachedResponse == null)
                return null;

            cachedResponse.OperationId = completionRequest.OperationId!;
            var contentArtifactsList = new List<ContentArtifact>(cachedResponse.ContentArtifacts ??= [])
            {
                new() {
                    Id = "CachedResponse",
                    Title = "Cached Response",
                    Source = "SemanticCache"
                }
            };
            cachedResponse.ContentArtifacts = [.. contentArtifactsList];

            return cachedResponse;
        }

        private async Task SetCompletionResponseInCache(string instanceId, string agentName, CompletionResponse completionResponse)
        {
            try
            {
                if (!_semanticCacheService.HasCacheForAgent(instanceId, agentName))
                    await _semanticCacheService.InitializeCacheForAgent(
                        instanceId,
                        agentName,
                        _agent == null
                            ? _longRunningOperationContext!.SemanticCacheSettings!
                            : _agent!.CacheSettings!.SemanticCacheSettings!);

                await _semanticCacheService.SetCompletionResponseInCache(_instanceId, agentName, completionResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while setting the completion response in the semantic cache for operation {OperationId} and agent {AgentName} in instance {InstanceId}.",
                    completionResponse.OperationId,
                    _agent!.Name,
                    _instanceId);
            }
        }

        private async Task<LLMCompletionRequest> GetLLMCompletionRequest(CompletionRequest completionRequest) =>
            new LLMCompletionRequest
            {
                OperationId = completionRequest.OperationId,
                UserPrompt = completionRequest.UserPrompt!,
                UserPromptRewrite = completionRequest.UserPromptRewrite,
                MessageHistory = completionRequest.MessageHistory,
                Attachments = await GetAttachmentPaths(completionRequest.Attachments),
                Agent = _agent!,
                Objects = _explodedObjects!
            };

        private async Task<List<AttachmentProperties>> GetAttachmentPaths(List<string> attachmentObjectIds)
        {
            if (attachmentObjectIds.Count == 0)
                return [];

            var attachments = attachmentObjectIds
                .ToAsyncEnumerable()
                .SelectAwait(async x => await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(x, _callContext.CurrentUserIdentity!));

            List<AttachmentProperties> result = [];            
            await foreach (var attachment in attachments)
            {
                if (string.IsNullOrWhiteSpace(attachment.SecondaryProvider))
                {
                    result.Add(new AttachmentProperties
                    {
                        OriginalFileName = attachment.OriginalFileName,
                        ContentType = attachment.ContentType!,
                        Provider = ResourceProviderNames.FoundationaLLM_Attachment,
                        ProviderFileName = attachment.Path,
                        ProviderStorageAccountName = _attachmentResourceProvider.StorageAccountName
                    });
                }
                else
                {
                    var useAttachmentPath = (attachment.ContentType ?? string.Empty).StartsWith("image/", StringComparison.OrdinalIgnoreCase);

                    var fileMapping = await _azureOpenAIResourceProvider.GetResourceAsync<AzureOpenAIFileMapping>(
                        _instanceId,
                        attachment.SecondaryProviderObjectId!,
                        _callContext.CurrentUserIdentity!);

                    if (fileMapping.FileRequiresVectorization)
                    {
                        if (string.IsNullOrWhiteSpace(_openAIVectorStoreId))
                            throw new OrchestrationException($"The file {attachment.OriginalFileName} with file id {fileMapping.OpenAIFileId!} requires vectorization but the vector store id is invalid.");

                        var vectorizationResult = await _gatewayClient!.CreateAgentCapability(
                            _instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            new()
                            {
                                { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, false },
                                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, fileMapping.OpenAIEndpoint },
                                { OpenAIAgentCapabilityParameterNames.AddOpenAIFileToVectorStore, fileMapping.FileRequiresVectorization },
                                { OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, _openAIVectorStoreId! },
                                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileMapping.OpenAIFileId! }
                            });

                        vectorizationResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess, out var vectorizationSuccessObject);
                        var vectorizationSuccess = ((JsonElement)vectorizationSuccessObject!).Deserialize<bool>();

                        if (!vectorizationSuccess)
                            throw new OrchestrationException($"The vectorization of file {attachment.OriginalFileName} with file id {fileMapping.OpenAIFileId!} into the vector store with id {_openAIVectorStoreId} failed.");
                    }

                    result.Add(new AttachmentProperties
                    {
                        OriginalFileName = attachment.OriginalFileName,
                        ContentType = attachment.ContentType!,
                        Provider = useAttachmentPath
                            ? ResourceProviderNames.FoundationaLLM_Attachment
                            : attachment.SecondaryProvider!,
                        ProviderFileName = useAttachmentPath
                            ? attachment.Path
                            : fileMapping.OpenAIFileId!,
                        ProviderStorageAccountName = useAttachmentPath
                            ? _attachmentResourceProvider.StorageAccountName
                            : null
                    });
                }
            }

            return result;
        }

        private async Task<CompletionResponse> GetCompletionResponse(string operationId, LLMCompletionResponse llmCompletionResponse)
        {
            if (llmCompletionResponse.ContentArtifacts != null)
            {
                llmCompletionResponse.ContentArtifacts = llmCompletionResponse.ContentArtifacts
                    .GroupBy(c => c.Filepath)
                    .Select(g => g.First())
                    .ToArray();
            }

            if(llmCompletionResponse.Errors!=null && llmCompletionResponse.Errors.Length>0)
            {
                string errorString = string.Join(Environment.NewLine, llmCompletionResponse.Errors);
                _logger.LogError($"Error in completion response: {errorString}");
                
            }

            return new CompletionResponse
            {
                OperationId = operationId,
                Completion = llmCompletionResponse.Completion,
                Content = llmCompletionResponse.Content != null ? await TransformContentItems(llmCompletionResponse.Content) : null,
                UserPrompt = llmCompletionResponse.UserPrompt!,
                UserPromptRewrite = llmCompletionResponse.UserPromptRewrite,
                ContentArtifacts = llmCompletionResponse.ContentArtifacts,
                FullPrompt = llmCompletionResponse.FullPrompt,
                PromptTemplate = llmCompletionResponse.PromptTemplate,
                AgentName = llmCompletionResponse.AgentName,
                PromptTokens = llmCompletionResponse.PromptTokens,
                CompletionTokens = llmCompletionResponse.CompletionTokens,
                AnalysisResults = llmCompletionResponse.AnalysisResults                
            };
        }

        private async Task<List<MessageContentItemBase>> TransformContentItems(List<MessageContentItemBase> contentItems)
        {
            List<AzureOpenAIFileMapping> newFileMappings = [];
            if (contentItems == null || contentItems.Count == 0)
                return [];

            if (contentItems.All(ci => ci.AgentCapabilityCategory == AgentCapabilityCategoryNames.FoundationaLLMKnowledgeManagement))
                return contentItems;

            var result = contentItems.Select(ci => TransformContentItem(ci, newFileMappings)).ToList();
            var upsertOptions = new ResourceProviderUpsertOptions
            {
                Parameters = {
                    [AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId] = _agentObjectId                    
                }
            };

            foreach (var fileMapping in newFileMappings)
            {
                await _azureOpenAIResourceProvider.UpsertResourceAsync<AzureOpenAIFileMapping, ResourceProviderUpsertResult<AzureOpenAIFileMapping>>(
                _instanceId,
                fileMapping,
                _callContext.CurrentUserIdentity!,
                upsertOptions);
            }

            return result;
        }

        private MessageContentItemBase TransformContentItem(MessageContentItemBase contentItem, List<AzureOpenAIFileMapping> newFileMappings) =>
            contentItem.AgentCapabilityCategory switch
            {
                AgentCapabilityCategoryNames.OpenAIAssistants => TransformOpenAIAssistantsContentItem(contentItem, newFileMappings),
                AgentCapabilityCategoryNames.FoundationaLLMKnowledgeManagement => TransformFoundationaLLMKnowledgeManagementContentItem(contentItem),
                _ => throw new OrchestrationException($"The agent capability category {contentItem.AgentCapabilityCategory} is not supported.")
            };

        #region OpenAI Assistants content items

        private MessageContentItemBase TransformOpenAIAssistantsContentItem(MessageContentItemBase contentItem, List<AzureOpenAIFileMapping> newFileMappings) =>
            contentItem switch
            {
                OpenAIImageFileMessageContentItem openAIImageFile => TransformOpenAIAssistantsImageFile(openAIImageFile, newFileMappings),
                OpenAITextMessageContentItem openAITextMessage => TransformOpenAIAssistantsTextMessage(openAITextMessage, newFileMappings),
                _ => throw new OrchestrationException($"The content item type {contentItem.GetType().Name} is not supported.")
            };

        private OpenAIImageFileMessageContentItem TransformOpenAIAssistantsImageFile(OpenAIImageFileMessageContentItem openAIImageFile, List<AzureOpenAIFileMapping> newFileMappings)
        {
            newFileMappings.Add(new AzureOpenAIFileMapping
            {
                Name = openAIImageFile.FileId!,
                Id = openAIImageFile.FileId!,
                UPN = _callContext.CurrentUserIdentity!.UPN!,
                InstanceId = _instanceId,
                FileObjectId = $"/instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{AzureOpenAIResourceTypeNames.FileMappings}/{openAIImageFile.FileId}",
                OriginalFileName = openAIImageFile.FileId!,
                FileContentType = "image/png",
                OpenAIEndpoint = _agentWorkflowMainAIModelAPIEndpoint,
                OpenAIFileId = openAIImageFile.FileId!,
                OpenAIAssistantsFileGeneratedOn = DateTimeOffset.UtcNow
            });
            openAIImageFile.FileUrl = $"{{{{fllm_base_url}}}}/instances/{_instanceId}/files/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{openAIImageFile.FileId}";
            return openAIImageFile;
        }

        private OpenAIFilePathContentItem TransformOpenAIAssistantsFilePath(OpenAIFilePathContentItem openAIFilePath, List<AzureOpenAIFileMapping> newFileMappings)
        {
            if (!string.IsNullOrWhiteSpace(openAIFilePath.FileId))
            {
                // Empty file ids occur when dealing with file search annotations.
                // Looks like the assistant is providing "internal" RAG pattern references to vectorized text chunks that were included in the context.
                // In this case, we should not generate a file mapping as it will result in invalid file urls.
                newFileMappings.Add(new AzureOpenAIFileMapping
                {
                    Name = openAIFilePath.FileId!,
                    Id = openAIFilePath.FileId!,
                    UPN = _callContext.CurrentUserIdentity!.UPN!,
                    InstanceId = _instanceId,
                    FileObjectId = $"/instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{AzureOpenAIResourceTypeNames.FileMappings}/{openAIFilePath.FileId}",
                    OriginalFileName = openAIFilePath.FileId!,
                    FileContentType = "application/octet-stream",
                    OpenAIEndpoint = _agentWorkflowMainAIModelAPIEndpoint,
                    OpenAIFileId = openAIFilePath.FileId!,
                    OpenAIAssistantsFileGeneratedOn = DateTimeOffset.UtcNow
                });
                openAIFilePath.FileUrl = $"{{{{fllm_base_url}}}}/instances/{_instanceId}/files/{ResourceProviderNames.FoundationaLLM_AzureOpenAI}/{openAIFilePath.FileId}";
            }
            else
                openAIFilePath.FileUrl = null;

            return openAIFilePath;
        }

        private OpenAITextMessageContentItem TransformOpenAIAssistantsTextMessage(OpenAITextMessageContentItem openAITextMessage, List<AzureOpenAIFileMapping> newFileMappings)
        {
            var pattern = new Regex(@"\【[0-9:]+†.+?\】");

            openAITextMessage.Value = pattern.Replace(openAITextMessage.Value!, string.Empty);

            openAITextMessage.Annotations = openAITextMessage.Annotations
                .Where(a => !pattern.Match(a.Text!).Success)
                .Select(a => TransformOpenAIAssistantsFilePath(a, newFileMappings))
                .ToList();

            #region Replace code interpreter placeholders with file urls

            // Code interpreter placeholders are assumed to be in the form of (sandbox:file-id).
            // They are expected to be unique and have a valid corresponding file url.
            var codeInterpreterPlaceholders = openAITextMessage.Annotations
                .Where(a => !string.IsNullOrWhiteSpace(a.FileUrl) && !string.IsNullOrWhiteSpace(a.Text))
                .DistinctBy(a => a.Text)
                .ToDictionary(
                    a => $"({a.Text!})",
                    a => $"({a.FileUrl})");
            

            var input = openAITextMessage.Value!;
            var regex = new Regex(@"\(sandbox:[^)]*\)");
            var matches = regex.Matches(input);

            if (matches.Count == 0)
                return openAITextMessage;

            Match? previousMatch = null;
            List<string> output = [];

            foreach (Match match in matches)
            {
                var startIndex = previousMatch == null ? 0 : previousMatch.Index + previousMatch.Length;
                output.Add(input[startIndex..match.Index]);
                var token = input.Substring(match.Index, match.Length);
                if (codeInterpreterPlaceholders.TryGetValue(token, out var replacement))
                    output.Add(replacement);
                else
                    output.Add(token);

                previousMatch = match;
            }

            output.Add(input.Substring(previousMatch!.Index + previousMatch.Length));

            openAITextMessage.Value = string.Join("", output);

            #endregion

            #region Replace file search placeholders with empty strings

            // File search placeholders are assumed to be unique and not have a corresponding file url.
            var fileSearchPlaceholders = openAITextMessage.Annotations
                .Where(a => string.IsNullOrWhiteSpace(a.FileUrl) && !string.IsNullOrWhiteSpace(a.Text))
                .DistinctBy(a => a.Text)
                .Select(a => a.Text!)
                .ToList();

            foreach (var fileSearchPlaceholder in fileSearchPlaceholders)
            {
                openAITextMessage.Value = openAITextMessage.Value.Replace(fileSearchPlaceholder, string.Empty);
            }

            #endregion

            return openAITextMessage;
        }

        #endregion

        #region FoundationaLLM Knowledge Management content items

        private MessageContentItemBase TransformFoundationaLLMKnowledgeManagementContentItem(MessageContentItemBase contentItem) =>
            contentItem;

        #endregion
    }
}
