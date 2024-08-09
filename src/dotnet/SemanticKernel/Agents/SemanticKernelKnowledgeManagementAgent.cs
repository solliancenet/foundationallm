using System.ComponentModel;
using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using FoundationaLLM.SemanticKernel.Core.Filters;
using FoundationaLLM.SemanticKernel.Core.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System.Net;
using System.Text.Json;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.SemanticKernel.Connectors.AzureCosmosDBNoSQL;
using Microsoft.SemanticKernel.Connectors.Postgres;
using System.Runtime;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0060

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// The Knowledge Management agent.
    /// </summary>
    /// <param name="request">The <see cref="KnowledgeManagementCompletionRequest"/> to be processed by the agent.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class SemanticKernelKnowledgeManagementAgent(
        LLMCompletionRequest request,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory,
        IHttpClientFactoryService httpClientFactoryService) : SemanticKernelAgentBase(request, resourceProviderServices, loggerFactory.CreateLogger<SemanticKernelKnowledgeManagementAgent>())
    {
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private string _textEmbeddingDeploymentName = string.Empty;
        private string _textEmbeddingEndpoint = string.Empty;
        private string _indexerName = string.Empty;
        private IndexerType _indexerType = IndexerType.AzureAISearchIndexer;
        private string _indexName = string.Empty;
        private string _prompt = string.Empty;
        private Dictionary<string, string>? _agentDescriptions = [];
        private AzureAISearchIndexingServiceSettings? _azureAISearchIndexingServiceSettings = null;
        private AzureCosmosDBNoSQLIndexingServiceSettings? _azureCosmosDBNoSQLIndexingServiceSettings = null;
        private PostgresIndexingServiceSettings? _postgresIndexingServiceSettings = null;

        protected override async Task ExpandAndValidateAgent()
        {
            _agentDescriptions = _request.OtherAgentsDescriptions;
            _prompt = _request.Prompt.Prefix!;

            #region Vectorization (text embedding and indexing) - optional

            var textEmbeddingProfile = _request.TextEmbeddingProfile;
            var indexingProfiles = _request.IndexingProfiles;

            if (textEmbeddingProfile != null)
            {
                // Get the text embedding ai model for deployment name and endpoint URL from its API endpoint configuration.
               
                if (!_request.Objects.TryGetValue(
                        textEmbeddingProfile.EmbeddingAIModelObjectId, out var embeddingAIModelObject))
                    throw new SemanticKernelException($"The AI Model object with id {textEmbeddingProfile.EmbeddingAIModelObjectId} is missing from the request's objects.");

                // Validate deployment name and endpoint url are present.
                var embeddingAIModel = embeddingAIModelObject is JsonElement embeddingAIModelJsonElement
                    ? embeddingAIModelJsonElement.Deserialize<EmbeddingAIModel>()
                    : embeddingAIModelObject as EmbeddingAIModel;

                if (embeddingAIModel == null)
                    throw new SemanticKernelException($"The AI Model object with id {textEmbeddingProfile.EmbeddingAIModelObjectId} provided in the request's objects is invalid.");

                _deploymentName = embeddingAIModel.DeploymentName!;
                //check for deployment name override from Settings
                if(textEmbeddingProfile.Settings is not null && textEmbeddingProfile.Settings.TryGetValue("deployment_name", out string? deploymentNameOverride))
                {
                    _deploymentName = deploymentNameOverride;
                }
                if(!_request.Objects.TryGetValue(
                            embeddingAIModel.EndpointObjectId, out var embeddingAIModelEndpointObject))
                        throw new SemanticKernelException($"The AI Model API endpoint configuration object is missing from the request's objects.");

                // Get the endpoint configuration object, ensure it can be deserialized to an APIEndpointConfiguration. URL is required on APIEndpointConfiguration, no further validation required.
                var embeddingAIModelEndpoint = embeddingAIModelEndpointObject is JsonElement embeddingAIModelEndpointJsonElement
                    ? embeddingAIModelEndpointJsonElement.Deserialize<APIEndpointConfiguration>()
                    : embeddingAIModelEndpointObject as APIEndpointConfiguration;

                if (embeddingAIModelEndpoint == null)
                    throw new SemanticKernelException("The AI Model API endpoint configuration object provided in the request's objects is invalid.");

                if (string.IsNullOrWhiteSpace(embeddingAIModelEndpoint.Url))
                    throw new SemanticKernelException($"The AI Model API endpoint configuration URL is invalid.");

                _textEmbeddingEndpoint = embeddingAIModelEndpoint.Url;
            }

            if ((indexingProfiles ?? []).Count > 0)
            {
                var indexingProfile = indexingProfiles![0];

                if (indexingProfile == null
                    || !await ValidateAndMapIndexingProfileConfiguration(indexingProfile)
                    || indexingProfile.Settings == null
                    || !indexingProfile.Settings.TryGetValue("IndexName", out var indexName)
                    || string.IsNullOrWhiteSpace(indexName))
                    throw new SemanticKernelException("The indexing profile object provided in the request's objects is invalid.", StatusCodes.Status400BadRequest);

                _indexerName = indexingProfile.Indexer.ToString();
                _indexName = indexName;
                _indexerType = indexingProfile.Indexer;
            }

            #endregion
        }

        /// <summary>
        /// Validates and maps the configuration of the indexing profile to the corresponding settings.
        /// </summary>
        /// <returns>True if valid.</returns>
        protected async Task<bool> ValidateAndMapIndexingProfileConfiguration(IndexingProfile indexingProfile)
        {
            var valid = false;
            var connectionStringConfigurationItem = string.Empty;
            var authenticationType = string.Empty;
            var indexingEndpointConfigurationItem = string.Empty;
            var databaseNameConfigurationItem = string.Empty;
            var vectorSizeConfigurationItem = string.Empty;

            switch (indexingProfile.Indexer)
            {
                case IndexerType.AzureAISearchIndexer:
                    if (!_request.Objects.TryGetValue(
                        indexingProfile.IndexingAPIEndpointConfigurationObjectId, out var endpointConfigurationObject))
                        throw new SemanticKernelException($"The indexing profile endpoint object with id {indexingProfile.IndexingAPIEndpointConfigurationObjectId} is missing from the request's objects.");

                    var endpointConfiguration = endpointConfigurationObject is JsonElement endpointConfigurationJsonElement
                        ? endpointConfigurationJsonElement.Deserialize<APIEndpointConfiguration>()
                        : endpointConfigurationObject as APIEndpointConfiguration;

                    if(endpointConfiguration == null)
                        throw new SemanticKernelException($"The indexing profile endpoint object with id {indexingProfile.IndexingAPIEndpointConfigurationObjectId} provided in the request's objects is invalid.");

                    // URL and Authentication type is already required on the APIEndpointConfiguration, no further validation required.                                        
                    _azureAISearchIndexingServiceSettings = new AzureAISearchIndexingServiceSettings
                    {
                        Endpoint = endpointConfiguration.Url,
                        AuthenticationType = endpointConfiguration.AuthenticationType
                    };
                    
                    break;                
                case IndexerType.AzureCosmosDBNoSQLIndexer:
                    valid = indexingProfile.ConfigurationReferences != null
                        && indexingProfile.ConfigurationReferences.TryGetValue("ConnectionString",
                                                           out connectionStringConfigurationItem)
                        && !string.IsNullOrWhiteSpace(connectionStringConfigurationItem)
                        && indexingProfile.ConfigurationReferences.TryGetValue("VectorDatabase",
                                                           out databaseNameConfigurationItem)
                        && !string.IsNullOrWhiteSpace(databaseNameConfigurationItem);
                    if (valid)
                    {
                        _azureCosmosDBNoSQLIndexingServiceSettings = new AzureCosmosDBNoSQLIndexingServiceSettings
                        {
                            ConnectionString = await GetConfigurationValue(connectionStringConfigurationItem!),
                            VectorDatabase = await GetConfigurationValue(databaseNameConfigurationItem!)
                        };
                    }
                    break;
                case IndexerType.PostgresIndexer:
                    valid = indexingProfile.ConfigurationReferences != null
                        && indexingProfile.ConfigurationReferences.TryGetValue("ConnectionString",
                                                                                      out connectionStringConfigurationItem)
                        && !string.IsNullOrWhiteSpace(connectionStringConfigurationItem)
                        && indexingProfile.ConfigurationReferences.TryGetValue("VectorSize",
                                                                                      out vectorSizeConfigurationItem)
                        && !string.IsNullOrWhiteSpace(vectorSizeConfigurationItem);
                    if (valid)
                    {
                        _postgresIndexingServiceSettings = new PostgresIndexingServiceSettings
                        {
                            ConnectionString = await GetConfigurationValue(connectionStringConfigurationItem!),
                            VectorSize = await GetConfigurationValue(vectorSizeConfigurationItem!)
                        };
                    }
                    break;                
            }

            return valid;
        }

        protected override async Task<LLMCompletionResponse> BuildResponseWithAzureOpenAI()
        {
            try
            {
                var kernel = BuildKernel();

                // Use observability features to capture the fully rendered prompt.
                var promptFilter = new DefaultPromptFilter();
                kernel.PromptRenderFilters.Add(promptFilter);

                var arguments = new KernelArguments()
                {
                    ["userPrompt"] = _request.UserPrompt!,
                    ["messageHistory"] = _request.MessageHistory
                };

                var result = await kernel.InvokePromptAsync(_prompt, arguments);

                var completion = result.GetValue<string>()!;
                var completionUsage = (result.Metadata!["Usage"] as CompletionsUsage)!;

                return new LLMCompletionResponse
                {
                    OperationId = _request.OperationId,
                    Completion = completion,
                    UserPrompt = _request.UserPrompt!,
                    FullPrompt = promptFilter.RenderedPrompt,
                    AgentName = _request.Agent.Name,
                    PromptTokens = completionUsage!.PromptTokens,
                    CompletionTokens = completionUsage.CompletionTokens,
                };
            }
            catch (Exception ex)
            {
                var message = "The response building process encountered an error.";
                _logger.LogError(ex, message);
                throw new SemanticKernelException(message, StatusCodes.Status500InternalServerError);
            }
        }

        private Kernel BuildKernel()
        {
            var credential = DefaultAuthentication.AzureCredential;

            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<ILoggerFactory>(_loggerFactory);

            // Create an HTTP client with to pass into AddAzureOpenAIChatCompletion           
            var httpClient = httpClientFactoryService.CreateUnregisteredClient(TimeSpan.FromMinutes(20));

            builder.AddAzureOpenAIChatCompletion(
                _deploymentName,
                _endpointUrl,
                credential,
                null,
                null,
                httpClient
               );
            builder.Services.ConfigureHttpClientDefaults(c =>
            {
                // Use a standard resiliency policy configured to retry on 429 (too many requests).
                c.AddStandardResilienceHandler().Configure(o =>
                {
                    o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                });
            });
            var kernel = builder.Build();

            // If the vectorization properties are not set, we are not going to import the context building capabilities.
            switch (_indexerType)
            {
                case IndexerType.AzureAISearchIndexer:
                    if (_azureAISearchIndexingServiceSettings != null &&
                        !string.IsNullOrWhiteSpace(_azureAISearchIndexingServiceSettings.Endpoint))
                    {
                        var memory = new MemoryBuilder()
                            .WithMemoryStore(new AzureAISearchMemoryStore(_azureAISearchIndexingServiceSettings.Endpoint, credential))
                            .WithAzureOpenAITextEmbeddingGeneration(_textEmbeddingDeploymentName, _textEmbeddingEndpoint, credential)
                            .Build();

                        kernel.ImportPluginFromObject(new KnowledgeManagementContextPlugin(memory, _indexName));
                    }
                    break;
                case IndexerType.AzureCosmosDBNoSQLIndexer:
                    if (_azureCosmosDBNoSQLIndexingServiceSettings != null &&
                        !string.IsNullOrWhiteSpace(_azureCosmosDBNoSQLIndexingServiceSettings.ConnectionString))
                    {
                        var memory = new MemoryBuilder()
                            .WithMemoryStore(new AzureCosmosDBNoSQLMemoryStore(
                                _azureCosmosDBNoSQLIndexingServiceSettings.ConnectionString,
                                _azureCosmosDBNoSQLIndexingServiceSettings.VectorDatabase!,
                                _azureCosmosDBNoSQLIndexingServiceSettings.VectorEmbeddingPolicy,
                                _azureCosmosDBNoSQLIndexingServiceSettings.IndexingPolicy))
                            .WithAzureOpenAITextEmbeddingGeneration(_textEmbeddingDeploymentName, _textEmbeddingEndpoint, credential)
                            .Build();

                        kernel.ImportPluginFromObject(new KnowledgeManagementContextPlugin(memory, _indexName));
                    }
                    break;
                case IndexerType.PostgresIndexer:
                    if (_postgresIndexingServiceSettings != null &&
                        !string.IsNullOrWhiteSpace(_postgresIndexingServiceSettings.ConnectionString))
                    {
                        _ = int.TryParse(_postgresIndexingServiceSettings.VectorSize, out var vectorSize);
                        var memory = new MemoryBuilder()
                            .WithMemoryStore(new PostgresMemoryStore(
                                _postgresIndexingServiceSettings.ConnectionString,
                                vectorSize))
                            .WithAzureOpenAITextEmbeddingGeneration(_textEmbeddingDeploymentName, _textEmbeddingEndpoint, credential)
                            .Build();

                        kernel.ImportPluginFromObject(new KnowledgeManagementContextPlugin(memory, _indexName));
                    }
                    break;
            }

            if (_agentDescriptions != null && _agentDescriptions.Count > 0)
            {
                kernel.ImportPluginFromObject(new AgentConversationPlugin(_agentDescriptions));
            }

            return kernel;
        }
    }
}
