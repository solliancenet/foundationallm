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
            var agent = _request.Agent as KnowledgeManagementAgent;

            if (agent!.OrchestrationSettings!.AgentParameters == null)
                throw new SemanticKernelException("The agent parameters are missing in the orchestration settings.", StatusCodes.Status400BadRequest);

            #region Other agent descriptions

            if (agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        "AllAgents", out var allAgentDescriptions))
            {
                _agentDescriptions = allAgentDescriptions is JsonElement allAgentDescriptionsJsonElement
                    ? allAgentDescriptionsJsonElement.Deserialize<Dictionary<string, string>>()
                    : allAgentDescriptions as Dictionary<string, string>;
            }

            #endregion

            #region Prompt

            if (string.IsNullOrWhiteSpace(agent.PromptObjectId))
                throw new SemanticKernelException("Invalid prompt object id.", StatusCodes.Status400BadRequest);

            if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                    agent.PromptObjectId, out var promptObject))
                throw new SemanticKernelException("The prompt object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

            var prompt = promptObject is JsonElement promptJsonElement
                ? promptJsonElement.Deserialize<MultipartPrompt>()
                : promptObject as MultipartPrompt;

            if (prompt == null
                || string.IsNullOrWhiteSpace(prompt.Prefix))
                throw new SemanticKernelException("The prompt object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

            _prompt = prompt.Prefix;

            #endregion

            #region Vectorization (text embedding and indexing) - optional

            if (!string.IsNullOrWhiteSpace(agent.Vectorization.TextEmbeddingProfileObjectId))
            {
                if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        agent.Vectorization.TextEmbeddingProfileObjectId, out var textEmbeddingProfileObject))
                    throw new SemanticKernelException("The text embedding profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

                var textEmbeddingProfile = textEmbeddingProfileObject is JsonElement textEmbeddingProfileJsonElement
                    ? textEmbeddingProfileJsonElement.Deserialize<TextEmbeddingProfile>()
                    : textEmbeddingProfileObject as TextEmbeddingProfile;

                if (textEmbeddingProfile == null
                    || textEmbeddingProfile.ConfigurationReferences == null
                    || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("DeploymentName", out var deploymentNameConfigurationItem)
                    || string.IsNullOrWhiteSpace(deploymentNameConfigurationItem)
                    || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("Endpoint", out var textEmbeddingEndpointConfigurationItem)
                    || string.IsNullOrWhiteSpace(textEmbeddingEndpointConfigurationItem))
                    throw new SemanticKernelException("The text embedding profile object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

                _textEmbeddingDeploymentName = textEmbeddingProfile.Settings != null
                    && textEmbeddingProfile.Settings.TryGetValue("deployment_name", out string? deploymentNameOverride)
                    && !string.IsNullOrWhiteSpace(deploymentNameOverride)
                    ? deploymentNameOverride
                    : await GetConfigurationValue(deploymentNameConfigurationItem);
                _textEmbeddingEndpoint = await GetConfigurationValue(textEmbeddingEndpointConfigurationItem);
            }

            if ((agent.Vectorization.IndexingProfileObjectIds ?? []).Count > 0)
            {
                if (string.IsNullOrEmpty(agent.Vectorization.IndexingProfileObjectIds[0]))
                    throw new SemanticKernelException("The indexing profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

                if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        agent.Vectorization.IndexingProfileObjectIds[0], out var indexingProfileObject))
                    throw new SemanticKernelException("The indexing profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

                var indexingProfile = indexingProfileObject is JsonElement indexingProfileJsonElement
                    ? indexingProfileJsonElement.Deserialize<IndexingProfile>()
                    : indexingProfileObject as IndexingProfile;

                if (indexingProfile == null
                    || !await ValidateAndMapIndexingProfileConfiguration(indexingProfile)
                    || indexingProfile.Settings == null
                    || !indexingProfile.Settings.TryGetValue("IndexName", out var indexName)
                    || string.IsNullOrWhiteSpace(indexName))
                    throw new SemanticKernelException("The indexing profile object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

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
                    valid = indexingProfile.ConfigurationReferences != null
                       && indexingProfile.ConfigurationReferences.TryGetValue("Endpoint",
                               out indexingEndpointConfigurationItem)
                       && !string.IsNullOrWhiteSpace(indexingEndpointConfigurationItem)
                       && indexingProfile.ConfigurationReferences.TryGetValue("AuthenticationType",
                                                       out authenticationType)
                       && !string.IsNullOrWhiteSpace(authenticationType);
                    if (valid)
                    {
                        _azureAISearchIndexingServiceSettings = new AzureAISearchIndexingServiceSettings
                        {
                            Endpoint = await GetConfigurationValue(indexingEndpointConfigurationItem!),
                            AuthenticationType = Enum.Parse<AzureAISearchAuthenticationTypes>(await GetConfigurationValue(authenticationType!))
                        };
                    }
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
                    Completion = completion,
                    UserPrompt = _request.UserPrompt!,
                    FullPrompt = promptFilter.RenderedPrompt,
                    AgentName = _request.Agent.Name,
                    PromptTokens = completionUsage!.PromptTokens,
                    CompletionTokens = completionUsage.CompletionTokens,
                    TotalTokens = completionUsage.TotalTokens
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
                _endpoint,
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
