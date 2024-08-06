using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Exceptions;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using FoundationaLLM.Gateway.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implements the FoundationaLLM Gateway service.
    /// </summary>
    /// <param name="armService">The <see cref="IAzureResourceManagerService"/> instance providing Azure Resource Manager services.</param>
    /// <param name="options">The options providing the <see cref="GatewayCoreSettings"/> object.</param>
    /// <param name="instanceSettings">Contains the FoundationaLLM instance configuration settings.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class GatewayCore(
        IAzureResourceManagerService armService,
        IOptions<GatewayCoreSettings> options,
        IOptions<InstanceSettings> instanceSettings,
        ILoggerFactory loggerFactory) : IGatewayCore
    {
        private readonly IAzureResourceManagerService _armService = armService;
        private readonly GatewayCoreSettings _settings = options.Value;
        private readonly InstanceSettings _instanceSettings = instanceSettings.Value;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
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
                                    _instanceSettings.Id,
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
        public async Task<TextEmbeddingResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest)
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
                }
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
        public async Task<TextEmbeddingResult> GetEmbeddingOperationResult(string instanceId, string operationId)
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
    }
}
