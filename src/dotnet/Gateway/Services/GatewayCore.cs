using Azure.AI.OpenAI;
using Azure.AI.Projects;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.AzureAI;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using FoundationaLLM.Gateway.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;
using System.Collections.Concurrent;
using System.Text.Json;

#pragma warning disable OPENAI001

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implements the FoundationaLLM Gateway service.
    /// </summary>
    /// <param name="armService">The <see cref="IAzureResourceManagerService"/> instance providing Azure Resource Manager services.</param>
    /// <param name="options">The options providing the <see cref="GatewayCoreSettings"/> object.</param>
    /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class GatewayCore(
        IAzureResourceManagerService armService,
        IOptions<GatewayCoreSettings> options,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory) : IGatewayCore
    {
        private readonly IAzureResourceManagerService _armService = armService;
        private readonly GatewayCoreSettings _settings = options.Value;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly IResourceProviderService _attachmentResourceProvider =
            resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
        private readonly IResourceProviderService _agentResourceProvider =
            resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
        private readonly ILogger<GatewayCore> _logger = loggerFactory.CreateLogger<GatewayCore>();

        private bool _initialized = false;

        private Dictionary<string, AzureOpenAIAccount> _azureOpenAIAccounts = [];
        private Dictionary<string, EmbeddingModelContext> _embeddingModels = [];

        private ConcurrentDictionary<string, EmbeddingOperationContext> _embeddingOperations = [];

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The Gateway core service is starting.");

            try
            {
                var openAIAccounts = _settings.AzureOpenAIAccounts.Split(";");

                foreach (var openAIAccount in openAIAccounts)
                {
                    _logger.LogInformation("Loading properties for the Azure OpenAI account with resource id {AccountResourceId}.", openAIAccount);

                    try
                    {
                        var accountProperties = await _armService.GetOpenAIAccountProperties(openAIAccount);
                        _azureOpenAIAccounts.Add(accountProperties.Name, accountProperties);

                        foreach (var deployment in accountProperties.Deployments)
                        {
                            if (deployment.CanDoEmbeddings)
                            {
                                var embeddingModelContext = new EmbeddingModelDeploymentContext(
                                    deployment,
                                    _loggerFactory);

                                if (!_embeddingModels.ContainsKey(deployment.ModelName))
                                    _embeddingModels[deployment.ModelName] = new EmbeddingModelContext(
                                        _embeddingOperations,
                                        _loggerFactory.CreateLogger<EmbeddingModelContext>())
                                    {
                                        ModelName = deployment.ModelName,
                                        DeploymentContexts = [embeddingModelContext]
                                    };
                                else
                                    _embeddingModels[deployment.ModelName].DeploymentContexts.Add(embeddingModelContext);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "There was an error while loading the properties for the Azure OpenAI account with resource id {AccountResourceId}.", openAIAccount);
                    }
                }

                _initialized = true;
                _logger.LogInformation("The Gateway core service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Gateway core did not start successfully due to an error.");
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The Gateway core service is stopping.");
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            _logger.LogInformation("The Gateway core service is executing.");

            var modelTasks = _embeddingModels.Values
                .Select(em => Task.Run(() => em.ProcessOperations(cancellationToken)))
                .ToArray();

            await Task.WhenAll(modelTasks);
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest, UnifiedUserIdentity userIdentity)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            if (!_embeddingModels.TryGetValue(embeddingRequest.EmbeddingModelName, out var embeddingModel))
                throw new GatewayException("The requested embedding model is not available.", StatusCodes.Status404NotFound);

            var operationId = Guid.NewGuid().ToString().ToLower();
            var embeddingOperationContext = new EmbeddingOperationContext
            {
                InputTextChunks = embeddingRequest.TextChunks.Select(tc => new TextChunk
                {
                    OperationId = operationId,
                    Position = tc.Position,
                    Content = tc.Content,
                    TokensCount = tc.TokensCount
                }).ToList(),
                Result = new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = operationId,
                    TextChunks = embeddingRequest.TextChunks.Select(tc => new TextChunk
                    {
                        Position = tc.Position
                    }).ToList(),
                    TokenCount = 0
                },
                Prioritized = embeddingRequest.Prioritized
            };

            embeddingModel.AddEmbeddingOperationContext(embeddingOperationContext);

            return await Task.FromResult(
                new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = embeddingOperationContext.Result.OperationId
                });
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingOperationResult(string instanceId, string operationId, UnifiedUserIdentity userIdentity)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            if (!_embeddingOperations.TryGetValue(operationId, out var operationContext))
                throw new GatewayException("The operation identifier was not found.", StatusCodes.Status404NotFound);

            if (operationContext.Result.Failed)
                return await Task.FromResult(new TextEmbeddingResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = operationContext.Result.ErrorMessage,
                    OperationId = operationId
                });
            else if (operationContext.Result.InProgress)
                return await Task.FromResult(new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = operationId
                });
            else
                return await Task.FromResult(operationContext.Result);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> CreateAgentCapability(string instanceId, string capabilityCategory, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object>? parameters = null)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            return capabilityCategory switch
            {
                AgentCapabilityCategoryNames.OpenAIAssistants => await CreateOpenAIAgentCapability(instanceId, capabilityName, userIdentity, parameters!),
                AgentCapabilityCategoryNames.AzureAIAgents => await CreateAzureAIAgentCapability(instanceId, capabilityName, userIdentity, parameters!),
                _ => throw new GatewayException($"The agent capability category {capabilityCategory} is not supported by the Gateway service.",
                   StatusCodes.Status400BadRequest),
            };
        }

        #region OpenAI Assistants API
        private async Task<Dictionary<string, object>> CreateOpenAIAgentCapability(string instanceId, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object> parameters)
        {
            Dictionary<string, object> result = [];
            var createAssistant = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistant, false);
            var createAssistantVectorStore = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistantVectorStore, false);
            var createAssistantThread = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistantThread, false);
            var createAssistantFile = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, false);
            var addAssistantFileToVectorStore = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.AddOpenAIFileToVectorStore, false);
            var removeAssistantFileFromVectorStore = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromVectorStore, false);
            var addAssistantFileToCodeInterpreter = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.AddOpenAIFileToCodeInterpreter, false);
            var removeAssistantFileFromCodeInterpreter = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromCodeInterpreter, false);

            if (createAssistant
                && string.IsNullOrEmpty(capabilityName))
                throw new GatewayException("The specified capability name is invalid when creating an Azure Open AI assistant.", StatusCodes.Status400BadRequest);

            var endpoint = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIEndpoint);
            var azureOpenAIAccount = _azureOpenAIAccounts.Values.FirstOrDefault(
                a => Uri.Compare(
                    new Uri(endpoint!),
                    new Uri(a.Endpoint),
                    UriComponents.Host,
                    UriFormat.SafeUnescaped,
                    StringComparison.OrdinalIgnoreCase) == 0)
                ?? throw new GatewayException($"The Gateway service is not configured to use the {endpoint} endpoint.");

            if (createAssistant)
            {
                var assistantClient = GetAzureOpenAIAssistantClient(azureOpenAIAccount.Endpoint);
                var vectorStoreClient = GetAzureOpenAIVectorStoreClient(azureOpenAIAccount.Endpoint);

                // Create the assistant-level vector store and assign it to the file search tool definition for the assistant.
                var vectorStoreResult = await vectorStoreClient.CreateVectorStoreAsync(true, new VectorStoreCreationOptions
                {
                    Name = capabilityName,
                    ExpirationPolicy = new OpenAI.VectorStores.VectorStoreExpirationPolicy
                    {
                        Anchor = VectorStoreExpirationAnchor.LastActiveAt,
                        Days = 365
                    }
                });
                var fileSearchToolResources = new FileSearchToolResources();
                fileSearchToolResources.VectorStoreIds.Add(vectorStoreResult.Value!.Id);

                var prompt = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIAssistantPrompt);
                var modelDeploymentName = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName);
                var azureOpenAIModel = azureOpenAIAccount.Deployments.FirstOrDefault(
                    d => string.Compare(
                        modelDeploymentName,
                        d.Name,
                        true) == 0)
                    ?? throw new GatewayException($"The Gateway service cannot find the {modelDeploymentName} model deployment in the account with endpoint {endpoint}.");

                var assistantResult = await assistantClient.CreateAssistantAsync(modelDeploymentName, new AssistantCreationOptions()
                {
                    Name = capabilityName,
                    Instructions = prompt,
                    Tools =
                    {
                        new OpenAI.Assistants.CodeInterpreterToolDefinition(),
                        new OpenAI.Assistants.FileSearchToolDefinition()
                    },
                    ToolResources = new OpenAI.Assistants.ToolResources()
                    {
                        FileSearch = fileSearchToolResources
                    }

                });

                var assistant = assistantResult.Value;
                _logger.LogInformation("Created assistant {AssistantName} with ID {AssistantId}.", assistant.Name, assistant.Id);
                result[OpenAIAgentCapabilityParameterNames.OpenAIAssistantId] = assistant.Id;
                result[OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId] = vectorStoreResult.Value!.Id;
            }

            if (createAssistantVectorStore)
            {
                var assistantClient = GetAzureOpenAIAssistantClient(azureOpenAIAccount.Endpoint);
                var vectorStoreClient = GetAzureOpenAIVectorStoreClient(azureOpenAIAccount.Endpoint);
                var assistantId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIAssistantId);

                var assistant = await assistantClient.GetAssistantAsync(assistantId);
                if (assistant.Value == null)
                {
                    throw new GatewayException($"The assistant with ID {assistantId} was not found.", StatusCodes.Status404NotFound);
                }

                var vectorStoreResult = await vectorStoreClient.CreateVectorStoreAsync(true, new VectorStoreCreationOptions
                {
                    Name = $"vs_{assistant.Value.Name}",
                    ExpirationPolicy = new OpenAI.VectorStores.VectorStoreExpirationPolicy
                    {
                        Anchor = VectorStoreExpirationAnchor.LastActiveAt,
                        Days = 365
                    }
                });

                var fileSearchToolResources = new FileSearchToolResources();
                fileSearchToolResources.VectorStoreIds.Add(vectorStoreResult.Value!.Id);

                // Update the assistant with the new vector store file search tool resource               
                var updateAssistantResult = await assistantClient.ModifyAssistantAsync(assistant.Value.Id, new AssistantModificationOptions
                {
                    ToolResources = new OpenAI.Assistants.ToolResources()
                    {
                        FileSearch = fileSearchToolResources
                    }
                });
                _logger.LogInformation("Added vector store {VectorStoreId} to file search tool for assistant {AssistantId}.", vectorStoreResult.Value!.Id, assistantId);
                result[OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId] = vectorStoreResult.Value!.Id;
            }

            if (createAssistantThread)
            {
                var assistantClient = GetAzureOpenAIAssistantClient(azureOpenAIAccount.Endpoint);
                var vectorStoreClient = GetAzureOpenAIVectorStoreClient(azureOpenAIAccount.Endpoint);

                var vectorStoreResult = await vectorStoreClient.CreateVectorStoreAsync(true, new VectorStoreCreationOptions
                {
                    ExpirationPolicy = new OpenAI.VectorStores.VectorStoreExpirationPolicy
                    {
                        Anchor = VectorStoreExpirationAnchor.LastActiveAt,
                        Days = 365
                    }
                });

                var fileSearchTool = new FileSearchToolResources();
                fileSearchTool.VectorStoreIds.Add(vectorStoreResult.Value!.Id);

                var threadResult = await assistantClient.CreateThreadAsync(new ThreadCreationOptions
                {
                    ToolResources = new OpenAI.Assistants.ToolResources()
                    {
                        FileSearch = fileSearchTool
                    }
                });
                var thread = threadResult.Value;
                var vectorStore = vectorStoreResult.Value;
                _logger.LogInformation("Created thread {ThreadId} with vector store {VectorStoreId}.", thread.Id, vectorStore.Id);
                result[OpenAIAgentCapabilityParameterNames.OpenAIAssistantThreadId] = thread.Id;
                result[OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId] = vectorStore.Id;
            }

            var fileId = GetParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIFileId, string.Empty);

            if (createAssistantFile)
            {
                var fileClient = GetAzureOpenAIFileClient(azureOpenAIAccount.Endpoint);
                var originalFileName = string.Empty;
                byte[]? fileContent = null;

                // check if it's an attachment or an agent file
                var attachmentObjectId = GetParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AttachmentObjectId, string.Empty);
                if (!string.IsNullOrEmpty(attachmentObjectId))
                {
                    var attachmentFile = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(attachmentObjectId, userIdentity, new ResourceProviderGetOptions { LoadContent = true });
                    originalFileName = attachmentFile.OriginalFileName;
                    fileContent = attachmentFile.Content;
                }
                else
                {
                    var agentFileObjectId = GetParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AgentFileObjectId, string.Empty);
                    var agentFile = await _agentResourceProvider.GetResourceAsync<Common.Models.ResourceProviders.Agent.AgentFiles.AgentFile>(agentFileObjectId, userIdentity, new ResourceProviderGetOptions { LoadContent = true });
                    originalFileName = agentFile.DisplayName;
                    fileContent = agentFile.Content;
                }
                if (string.IsNullOrWhiteSpace(originalFileName))
                {
                    throw new GatewayException("The request does not have a valid AttachmentObjectId or AgentFileObjectId parameter value.", StatusCodes.Status400BadRequest);
                }
                if (fileContent == null)
                {
                    throw new GatewayException("The file content is null.", StatusCodes.Status400BadRequest);
                }
                var fileResult = await fileClient.UploadFileAsync(
                    new MemoryStream(fileContent!),
                    originalFileName,
                    FileUploadPurpose.Assistants);
                var file = fileResult.Value;
                _logger.LogInformation("Uploaded file {FileName} as an OpenAI file with ID {FileId}.", originalFileName, file.Id);
                result[OpenAIAgentCapabilityParameterNames.OpenAIFileId] = file.Id;
                fileId = file.Id;
            }

            if (addAssistantFileToVectorStore)
            {
                var vectorStoreClient = GetAzureOpenAIVectorStoreClient(azureOpenAIAccount.Endpoint);
                var vectorStoreId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId);

                var vectorizationResult = await vectorStoreClient.AddFileToVectorStoreAsync(vectorStoreId, fileId, false);

                var startTime = DateTimeOffset.UtcNow;
                _logger.LogInformation("Started vectorization of file {FileId} in vector store {VectorStoreId}.", fileId, vectorStoreId);
                var fileAssociationResult = await vectorStoreClient.GetFileAssociationAsync(vectorStoreId, fileId);

                var maxPollingTimeExceeded = false;
                while (fileAssociationResult.Value.Status == VectorStoreFileAssociationStatus.InProgress)
                {
                    await Task.Delay(5000);
                    if ((DateTimeOffset.UtcNow - startTime).TotalSeconds >= _settings.AzureOpenAIAssistantsMaxVectorizationTimeSeconds)
                    {
                        maxPollingTimeExceeded = true;
                        break;
                    }
                    fileAssociationResult = await vectorStoreClient.GetFileAssociationAsync(vectorStoreId, fileId);
                }

                if (maxPollingTimeExceeded)
                {
                    _logger.LogWarning("The maximum polling time ({MaxPollingTime} seconds) was exceeded during the vectorization of file {FileId} in vector store {VectorStoreId}.",
                        _settings.AzureOpenAIAssistantsMaxVectorizationTimeSeconds, fileId, vectorStoreId);
                    result[OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess] = false;
                }
                else
                {                    
                    _logger.LogInformation("Completed vectorization of file {FileId} in vector store {VectorStoreId} in {TotalSeconds} with result {VectorizationResult}.",
                        fileId, vectorStoreId, (DateTimeOffset.UtcNow - startTime).TotalSeconds, fileAssociationResult.Value.Status);

                    if (fileAssociationResult.Value.Status == VectorStoreFileAssociationStatus.Failed)
                    {
                        _logger.LogError("The vectorization of file {FileId} in vector store {VectorStoreId} failed with error {ErrorMessage}.",
                                                       fileId, vectorStoreId, fileAssociationResult.Value.LastError);
                    }
                    
                    result[OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess] =
                        fileAssociationResult.Value.Status == VectorStoreFileAssociationStatus.Completed;
                }
            }

            if (removeAssistantFileFromVectorStore)
            {
                var vectorStoreClient = GetAzureOpenAIVectorStoreClient(azureOpenAIAccount.Endpoint);
                var vectorStoreId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId);
                // verify the file is in the vector store
                var associations = vectorStoreClient.GetFileAssociationsAsync(vectorStoreId);
                bool isRemoved = true; // return true if the file is not found in the vector store
                // iterate through associations as removing a file that is not in the vector store will throw an exception
                await foreach (var association in associations)
                {
                    if(association.FileId == fileId)
                    {
                        var vectorizationResult = await vectorStoreClient.RemoveFileFromStoreAsync(vectorStoreId, fileId);
                        isRemoved = vectorizationResult.Value.Removed;
                    }
                }
                _logger.LogInformation("Removed file {FileId} from vector store {VectorStoreId}.", fileId, vectorStoreId);
                result[OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess] = isRemoved;
            }

            if(addAssistantFileToCodeInterpreter)
            {
                var assistantClient = GetAzureOpenAIAssistantClient(azureOpenAIAccount.Endpoint);
                var assistantId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIAssistantId);
                var file = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIFileId);

                var assistant = await assistantClient.GetAssistantAsync(assistantId);
                if (assistant.Value == null)
                {
                    throw new GatewayException($"The assistant with ID {assistantId} was not found.", StatusCodes.Status404NotFound);
                }

                var codeInterpreterToolResources = assistant.Value.ToolResources.CodeInterpreter;

                if (!codeInterpreterToolResources.FileIds.Contains(file))
                {
                    codeInterpreterToolResources.FileIds.Add(file);
                }

                // Update the assistant with the new file in the code interpreter tool
                var updateAssistantResult = await assistantClient.ModifyAssistantAsync(assistant.Value.Id, new AssistantModificationOptions
                {
                    ToolResources = new OpenAI.Assistants.ToolResources()
                    {
                        CodeInterpreter = codeInterpreterToolResources
                    }
                });
                _logger.LogInformation("Added file {FileId} to code interpreter tool for assistant {AssistantId}.", file, assistantId);
                result[OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess] = true;

            }

            if (removeAssistantFileFromCodeInterpreter)
            {
                var assistantClient = GetAzureOpenAIAssistantClient(azureOpenAIAccount.Endpoint);
                var assistantId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIAssistantId);
                var file = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.OpenAIFileId);

                var assistant = await assistantClient.GetAssistantAsync(assistantId);
                if (assistant.Value == null)
                {
                    throw new GatewayException($"The assistant with ID {assistantId} was not found.", StatusCodes.Status404NotFound);
                }

                var codeInterpreterToolResources = assistant.Value.ToolResources.CodeInterpreter;

                if (codeInterpreterToolResources.FileIds.Contains(file))
                {
                    codeInterpreterToolResources.FileIds.Remove(file);
                }

                // Update the assistant with the new file in the code interpreter tool
                var updateAssistantResult = await assistantClient.ModifyAssistantAsync(assistant.Value.Id, new AssistantModificationOptions
                {
                    ToolResources = new OpenAI.Assistants.ToolResources()
                    {
                        CodeInterpreter = codeInterpreterToolResources
                    }
                });
                _logger.LogInformation("Removed file {FileId} from code interpreter tool for assistant {AssistantId}.", file, assistantId);
                result[OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess] = true;

            }
            return result;
        }

        private AzureOpenAIClient GetAzureOpenAIClient(string endpoint) =>
            new AzureOpenAIClient(
                new Uri(endpoint),
                ServiceContext.AzureCredential,
                new AzureOpenAIClientOptions
                {
                    NetworkTimeout = TimeSpan.FromSeconds(1000)
                });

        private AssistantClient GetAzureOpenAIAssistantClient(string endpoint) =>
            GetAzureOpenAIClient(endpoint).GetAssistantClient();

        private VectorStoreClient GetAzureOpenAIVectorStoreClient(string endpoint) =>
            GetAzureOpenAIClient(endpoint).GetVectorStoreClient();

        private OpenAIFileClient GetAzureOpenAIFileClient(string endpoint) =>
            GetAzureOpenAIClient(endpoint).GetOpenAIFileClient();

        #endregion

        #region Azure AI Agents Service
        private async Task<Dictionary<string, object>> CreateAzureAIAgentCapability(string instanceId, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object> parameters)
        {
            Dictionary<string, object> result = [];
            var createAgent = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.CreateAgent, false);
            var createAgentVectorStore = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.CreateVectorStore, false);
            var createAgentThread = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.CreateThread, false);
            var createAgentFile = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.CreateFile, false);
            var addAgentFileToVectorStore = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.AddFileToVectorStore, false);
            var removeAgentFileFromVectorStore = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.RemoveFileFromVectorStore, false);
            var addAgentFileToCodeInterpreter = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.AddFileToCodeInterpreter, false);
            var removeAgentFileFromCodeInterpreter = GetParameterValue<bool>(parameters, AzureAIAgentServiceCapabilityParameterNames.RemoveFileFromCodeInterpreter, false);

            if (createAgent
                && string.IsNullOrEmpty(capabilityName))
                throw new GatewayException("The specified capability name is invalid when creating an Azure AI Agent Service agent.", StatusCodes.Status400BadRequest);

            var projectConnectionString = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.ProjectConnectionString);
            var agentsClient = GetAgentsClient(projectConnectionString);

            if (createAgent)
            {
                // Create the agent-level vector store to assign to the file search tool definition for the agent.
                var vectorStoreResponse = await agentsClient.CreateVectorStoreAsync(
                        name: $"vs_{capabilityName}",
                        expiresAfter:
                            new Azure.AI.Projects.VectorStoreExpirationPolicy(
                                VectorStoreExpirationPolicyAnchor.LastActiveAt,
                                365
                            ));

                var fileSearchToolResource = new FileSearchToolResource();
                fileSearchToolResource.VectorStoreIds.Add(vectorStoreResponse.Value.Id);

                var prompt = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AgentPrompt);
                var modelDeploymentName = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AzureAIModelDeploymentName);

                var agentResponse = await agentsClient.CreateAgentAsync(
                    model: modelDeploymentName,
                    name: capabilityName,
                    instructions: prompt,
                    tools: new List<Azure.AI.Projects.ToolDefinition>()
                    {
                        new Azure.AI.Projects.CodeInterpreterToolDefinition(),
                        new Azure.AI.Projects.FileSearchToolDefinition()
                    },
                    toolResources: new Azure.AI.Projects.ToolResources()
                    {
                        FileSearch = fileSearchToolResource
                    });              

                var agent = agentResponse.Value;
                _logger.LogInformation("Created agent {AgentName} with ID {AgentId}.", agent.Name, agent.Id);
                result[AzureAIAgentServiceCapabilityParameterNames.AgentId] = agent.Id;
                result[AzureAIAgentServiceCapabilityParameterNames.VectorStoreId] = vectorStoreResponse.Value.Id;
            }

            if (createAgentVectorStore)
            {               
                var agentId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AgentId);
                var agentResponse = await agentsClient.GetAgentAsync(agentId);                
                if (agentResponse.Value == null)
                {
                    throw new GatewayException($"The agent with ID {agentId} was not found.", StatusCodes.Status404NotFound);
                }

                var vectorStoreResponse = await agentsClient.CreateVectorStoreAsync(
                        name: $"vs_{agentResponse.Value.Name}",
                        expiresAfter:
                            new Azure.AI.Projects.VectorStoreExpirationPolicy(
                                VectorStoreExpirationPolicyAnchor.LastActiveAt,
                                365
                            ));

                var fileSearchToolResource = new FileSearchToolResource();
                fileSearchToolResource.VectorStoreIds.Add(vectorStoreResponse.Value.Id);

                // Update the agent with the new vector store file search tool resource
                var updateAgentResponse = await agentsClient.UpdateAgentAsync(
                    agentId,
                    toolResources: new Azure.AI.Projects.ToolResources()
                    {
                        FileSearch = fileSearchToolResource
                    });             
                _logger.LogInformation("Added vector store {VectorStoreId} to file search tool for agent {AgentId}.", vectorStoreResponse.Value.Id, agentId);
                result[AzureAIAgentServiceCapabilityParameterNames.VectorStoreId] = vectorStoreResponse.Value.Id;
            }

            if (createAgentThread)
            {
                //Create thread-level vector store
                var vectorStoreResponse = await agentsClient.CreateVectorStoreAsync(                        
                        expiresAfter:
                            new Azure.AI.Projects.VectorStoreExpirationPolicy(
                                VectorStoreExpirationPolicyAnchor.LastActiveAt,
                                365
                            ));

                var fileSearchToolResource = new FileSearchToolResource();
                fileSearchToolResource.VectorStoreIds.Add(vectorStoreResponse.Value.Id);

                var threadResponse = await agentsClient.CreateThreadAsync(
                    toolResources: new Azure.AI.Projects.ToolResources()
                    {
                        FileSearch = fileSearchToolResource
                    }
                );
                               
                var thread = threadResponse.Value;
                var vectorStore = vectorStoreResponse.Value;
                _logger.LogInformation("Created thread {ThreadId} with vector store {VectorStoreId}.", thread.Id, vectorStore.Id);
                result[AzureAIAgentServiceCapabilityParameterNames.ThreadId] = thread.Id;
                result[AzureAIAgentServiceCapabilityParameterNames.VectorStoreId] = vectorStore.Id;
            }           

            if (createAgentFile)
            {
                var originalFileName = string.Empty;
                byte[]? fileContent = null;

                // check if it's an attachment or an agent file
                var attachmentObjectId = GetParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AttachmentObjectId, string.Empty);
                if (!string.IsNullOrEmpty(attachmentObjectId))
                {
                    var attachmentFile = await _attachmentResourceProvider.GetResourceAsync<AttachmentFile>(attachmentObjectId, userIdentity, new ResourceProviderGetOptions { LoadContent = true });
                    originalFileName = attachmentFile.OriginalFileName;
                    fileContent = attachmentFile.Content;
                }
                else
                {
                    var agentFileObjectId = GetParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AgentFileObjectId, string.Empty);
                    var agentFile = await _agentResourceProvider.GetResourceAsync<Common.Models.ResourceProviders.Agent.AgentFiles.AgentFile>(agentFileObjectId, userIdentity, new ResourceProviderGetOptions { LoadContent = true });
                    originalFileName = agentFile.DisplayName;
                    fileContent = agentFile.Content;
                }
                if (string.IsNullOrWhiteSpace(originalFileName))
                {
                    throw new GatewayException("The request does not have a valid AttachmentObjectId or AgentFileObjectId parameter value.", StatusCodes.Status400BadRequest);
                }
                if (fileContent == null)
                {
                    throw new GatewayException("The file content is null.", StatusCodes.Status400BadRequest);
                }

                var fileResponse = await agentsClient.UploadFileAsync(
                    new MemoryStream(fileContent!),
                    AgentFilePurpose.Agents,
                    originalFileName
                    );
               
                var file = fileResponse.Value;
                _logger.LogInformation("Uploaded file {FileName} as an Azure AI Agents Service file with ID {FileId}.", originalFileName, file.Id);
                result[AzureAIAgentServiceCapabilityParameterNames.FileId] = file.Id;                
            }  

            if (addAgentFileToVectorStore)
            {                
                var vectorStoreId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.VectorStoreId);
                var fileId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.FileId);               
                                
                var startTime = DateTimeOffset.UtcNow;
                _logger.LogInformation("Started vectorization of file {FileId} in vector store {VectorStoreId}.", fileId, vectorStoreId);

                var vectorStoreFileResponse = await agentsClient.GetVectorStoreFileAsync(
                    vectorStoreId: vectorStoreId,
                    fileId: fileId
                );
                var maxPollingTimeExceeded = false;
                while (vectorStoreFileResponse.Value.Status == VectorStoreFileStatus.InProgress)
                {
                    await Task.Delay(5000);
                    if ((DateTimeOffset.UtcNow - startTime).TotalSeconds >= _settings.AzureAIAgentServiceMaxVectorizationTimeSeconds)
                    {
                        maxPollingTimeExceeded = true;
                        break;
                    }
                    vectorStoreFileResponse = await agentsClient.GetVectorStoreFileAsync(
                        vectorStoreId: vectorStoreId,
                        fileId: fileId
                    );
                }
                if (maxPollingTimeExceeded)
                {
                    _logger.LogWarning("The maximum polling time ({MaxPollingTime} seconds) was exceeded during the vectorization of file {FileId} in vector store {VectorStoreId}.",
                        _settings.AzureAIAgentServiceMaxVectorizationTimeSeconds, fileId, vectorStoreId);
                    result[AzureAIAgentServiceCapabilityParameterNames.FileActionOnVectorStoreSuccess] = false;
                }
                else
                {
                    _logger.LogInformation("Completed vectorization of file {FileId} in vector store {VectorStoreId} in {TotalSeconds} with result {VectorizationResult}.",
                        fileId, vectorStoreId, (DateTimeOffset.UtcNow - startTime).TotalSeconds, vectorStoreFileResponse.Value.Status);
                    if (vectorStoreFileResponse.Value.Status == VectorStoreFileStatus.Failed)
                    {
                        _logger.LogError("The vectorization of file {FileId} in vector store {VectorStoreId} failed with error {ErrorMessage}.",
                                                       fileId, vectorStoreId, vectorStoreFileResponse.Value.LastError);
                    }
                    result[AzureAIAgentServiceCapabilityParameterNames.FileActionOnVectorStoreSuccess] =
                        vectorStoreFileResponse.Value.Status == VectorStoreFileStatus.Completed;
                }
            }

            if (removeAgentFileFromVectorStore)
            {
                var vectorStoreId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.VectorStoreId);
                var fileId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.FileId);
                var vectorizationResponse = await agentsClient.DeleteVectorStoreFileAsync(
                    vectorStoreId: vectorStoreId,
                    fileId: fileId
                );
                _logger.LogInformation("Removed file {FileId} from vector store {VectorStoreId}.", fileId, vectorStoreId);
                result[AzureAIAgentServiceCapabilityParameterNames.FileActionOnVectorStoreSuccess] = vectorizationResponse.Value.Deleted;
            }

            if (addAgentFileToCodeInterpreter)
            {
                var agentId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AgentId);
                var agentResponse = await agentsClient.GetAgentAsync(agentId);
                if (agentResponse.Value == null)
                {
                    throw new GatewayException($"The agent with ID {agentId} was not found.", StatusCodes.Status404NotFound);
                }
                var agent = agentResponse.Value;
                var fileId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.FileId);
                
                var codeInterpreterToolResource = agent.ToolResources.CodeInterpreter;

                if (!codeInterpreterToolResource.FileIds.Contains(fileId))
                {
                    codeInterpreterToolResource.FileIds.Add(fileId);
                }

                // Update the agent with the new file in the code interpreter tool
                _ = await agentsClient.UpdateAgentAsync(
                    agentId,
                    toolResources: new Azure.AI.Projects.ToolResources()
                    {
                        CodeInterpreter = codeInterpreterToolResource
                    }
                );
                
                _logger.LogInformation("Added file {FileId} to code interpreter tool for agent {AgentId}.", fileId, agentId);
                result[AzureAIAgentServiceCapabilityParameterNames.FileActionOnCodeInterpreterSuccess] = true;
            }

            if (removeAgentFileFromCodeInterpreter)
            {
                var agentId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.AgentId);
                var agentResponse = await agentsClient.GetAgentAsync(agentId);
                if (agentResponse.Value == null)
                {
                    throw new GatewayException($"The agent with ID {agentId} was not found.", StatusCodes.Status404NotFound);
                }
                var agent = agentResponse.Value;
                var fileId = GetRequiredParameterValue<string>(parameters, AzureAIAgentServiceCapabilityParameterNames.FileId);

                var codeInterpreterToolResource = agent.ToolResources.CodeInterpreter;

                if (!codeInterpreterToolResource.FileIds.Contains(fileId))
                {
                    codeInterpreterToolResource.FileIds.Remove(fileId);
                }

                // Update the agent with the file removed from the code interpreter tool
                _ = await agentsClient.UpdateAgentAsync(
                    agentId,
                    toolResources: new Azure.AI.Projects.ToolResources()
                    {
                        CodeInterpreter = codeInterpreterToolResource
                    }
                );
                _logger.LogInformation("Removed file {FileId} to code interpreter tool for agent {AgentId}.", fileId, agentId);
                result[AzureAIAgentServiceCapabilityParameterNames.FileActionOnCodeInterpreterSuccess] = true;                
            }
            return result;
        }

        /// <summary>
        /// Creates an instance of an Azure AI Agent Service client with the associated project connection string.
        /// </summary>
        /// <param name="projectConnectionString">The Azure AI project connection string.</param>
        /// <returns>AgentsClient for the Azure AI project.</returns>
        /// <remarks>Azure AI Agent Service currently supports only Entra authentication.</remarks>
        private AgentsClient GetAgentsClient(string projectConnectionString) =>
            new AgentsClient(projectConnectionString, ServiceContext.AzureCredential);

        #endregion

        private T GetParameterValue<T>(Dictionary<string, object> parameters, string parameterName, T defaultValue) =>
            parameters.TryGetValue(parameterName, out var parameterValueObject)
                ? ((JsonElement)parameterValueObject!).Deserialize<T>()
                    ?? defaultValue
                : defaultValue;

        private T GetRequiredParameterValue<T>(Dictionary<string, object> parameters, string parameterName) =>
            parameters.TryGetValue(parameterName, out var parameterValueObject)
                ? ((JsonElement)parameterValueObject!).Deserialize<T>()
                    ?? throw new GatewayException($"Could not load required parameter {parameterName}.", StatusCodes.Status400BadRequest)
                : throw new GatewayException($"The required parameter {parameterName} was not found.");

    }
}
