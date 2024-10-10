using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models;
using FoundationaLLM.Orchestration.Core.Orchestration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FoundationaLLM.Orchestration.Core.Services;

/// <summary>
/// OrchestrationService class.
/// </summary>
public class OrchestrationService : IOrchestrationService
{
    private readonly ILLMOrchestrationServiceManager _llmOrchestrationServiceManager;
    private readonly IAzureCosmosDBService _cosmosDBService;
    private readonly ICallContext _callContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrchestrationService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;

    /// <summary>
    /// Constructor for the Orchestration Service.
    /// </summary>
    /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
    /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> managing the internal and external LLM orchestration services.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> used to interact with the Cosmos DB database.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public OrchestrationService(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
        IAzureCosmosDBService cosmosDBService,
        ICallContext callContext,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {
        _resourceProviderServices = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        _llmOrchestrationServiceManager = llmOrchestrationServiceManager;
        _cosmosDBService = cosmosDBService;

        _callContext = callContext;
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<OrchestrationService>();
    }

    /// <inheritdoc/>
    public async Task<ServiceStatusInfo> GetStatus(string instanceId)
    {
        var subordinateStatuses = await _llmOrchestrationServiceManager.GetAggregateStatus(instanceId, _serviceProvider);
        return new ServiceStatusInfo
        {
            Name = ServiceNames.OrchestrationAPI,
            InstanceId = instanceId,
            InstanceName = ValidatedEnvironment.MachineName,
            Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
            Status = subordinateStatuses.All(s => s.Status!.Equals("ready", StringComparison.CurrentCultureIgnoreCase))
                ? "ready"
                : "partially_unavailable",
            SubordinateServices = subordinateStatuses
        };
    }

    /// <inheritdoc/>
    public async Task<CompletionResponse> GetCompletion(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            // TODO: Redesign and implement updated agent to agent conversation orchestration.
            //var conversationSteps = await GetAgentConversationSteps(instanceId, completionRequest.AgentName!, completionRequest.UserPrompt);
            //return await GetCompletionForAgentConversation(instanceId, completionRequest, conversationSteps);

            var orchestration = await OrchestrationBuilder.Build(
                instanceId,
                completionRequest.AgentName!,
                completionRequest,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _llmOrchestrationServiceManager,
                _cosmosDBService,
                _serviceProvider,
                _loggerFactory)
                ?? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration for agent [{completionRequest.AgentName ?? string.Empty}].");

            var completionResponse = await orchestration.GetCompletion(completionRequest);

            return completionResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completion from the orchestration service for {UserPrompt}.",
                completionRequest.UserPrompt);
            return new CompletionResponse
            {
                OperationId = completionRequest.OperationId!,
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt ?? string.Empty,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = [0f]
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
    {
        try
        {
            var orchestration = await OrchestrationBuilder.Build(
                instanceId,
                completionRequest.AgentName!,
                completionRequest,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _llmOrchestrationServiceManager,
                _cosmosDBService,
                _serviceProvider,
                _loggerFactory)
                ?? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration for agent [{completionRequest.AgentName ?? string.Empty}].");

            var operationResponse = await orchestration.StartCompletionOperation(completionRequest);

            return operationResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting the completion operation from the orchestration service for {UserPrompt}.",
                completionRequest.UserPrompt);
            return new LongRunningOperation
            {
                OperationId = completionRequest.OperationId!,
                Status = OperationStatus.Failed,
                StatusMessage = "The completion operation failed to start.",
                Result = new CompletionResponse
                {
                    OperationId = completionRequest.OperationId!,
                    Completion = "A problem on my side prevented me from responding.",
                    UserPrompt = completionRequest.UserPrompt ?? string.Empty,
                    PromptTokens = 0,
                    CompletionTokens = 0,
                    UserPromptEmbedding = [0f]
                }
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
    {
        try
        {
            var orchestration = await OrchestrationBuilder.BuildForStatus(
                instanceId,
                operationId,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _llmOrchestrationServiceManager,
                _cosmosDBService,
                _serviceProvider,
                _loggerFactory)
                ?? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration to retrieve the status of the operation with id {operationId}.");

            var operationStatus = await orchestration.GetCompletionOperationStatus(operationId);

            return operationStatus;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving the status of operation {OperationId} from the orchestration service.",
                operationId);
            return new LongRunningOperation
            {
                OperationId = operationId,
                Status = OperationStatus.Failed,
                StatusMessage = "Could not retrieve the status of the operation."
            };
        }
    }

    #region Obsolete: Agent to Agent Conversation Orchestration

    private async Task<CompletionResponse> GetCompletionForAgentConversation(
        string instanceId,
        CompletionRequest completionRequest,
        List<AgentConversationStep> agentConversationSteps)
    {
        var currentCompletionResponse = default(CompletionResponse);

        foreach (var conversationStep in agentConversationSteps)
        {
            var orchestration = await OrchestrationBuilder.Build(
                instanceId,
                conversationStep.AgentName,
                completionRequest,
                _callContext,
                _configuration,
                _resourceProviderServices,
                _llmOrchestrationServiceManager,
                _cosmosDBService,
                _serviceProvider,
                _loggerFactory);

            var stepCompletionRequest = new CompletionRequest
            {
                OperationId = completionRequest.OperationId,
                AgentName = conversationStep.AgentName,
                SessionId = completionRequest.SessionId,
                Settings = completionRequest.Settings,
                MessageHistory = completionRequest.MessageHistory,
                Attachments = completionRequest.Attachments,
                UserPrompt = currentCompletionResponse == null
                    ? conversationStep.UserPrompt
                    : $"{currentCompletionResponse.Completion}{Environment.NewLine}{conversationStep.UserPrompt}",
            };

            currentCompletionResponse = orchestration == null
                ? throw new OrchestrationException($"The orchestration builder was not able to create an orchestration for agent [{completionRequest.AgentName ?? string.Empty}].")
                : await orchestration.GetCompletion(stepCompletionRequest);

            //var newConversationSteps = await GetAgentConversationSteps(
            //    instanceId,
            //    currentCompletionResponse.AgentName!,
            //    currentCompletionResponse.Completion);
            //if (newConversationSteps.Count > 0
            //    && newConversationSteps.First().AgentName != currentCompletionResponse.AgentName)
            //    currentCompletionResponse =
            //        await GetCompletionForAgentConversation(instanceId, completionRequest, newConversationSteps);
        }

        return currentCompletionResponse!;
    }

    private async Task<List<AgentConversationStep>> GetAgentConversationSteps(string instanceId, string agentName, string userPrompt)
    {
        var currentPrompt = new StringBuilder();
        var result = new List<AgentConversationStep>();
        var currentAgentName = agentName;
        
        using (StringReader sr = new StringReader(userPrompt))
        {
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith('@'))
                {
                    var tokens = line.Split([' ', ',']);
                    var candidateAgentName = tokens.First().Replace("@", string.Empty);
                    var isValid = await ValidAgentName(instanceId, candidateAgentName);

                    if (isValid)
                    {
                        var newUserPrompt = currentPrompt.ToString().Trim();
                        if (!string.IsNullOrEmpty(newUserPrompt))
                            result.Add(new AgentConversationStep
                            {
                                AgentName = currentAgentName,
                                UserPrompt = newUserPrompt
                            });
                        currentAgentName = candidateAgentName;

                        currentPrompt = new StringBuilder();
                        var remainingLine = line.Substring(candidateAgentName.Length + 2);
                        if (!string.IsNullOrWhiteSpace(remainingLine))
                            currentPrompt.AppendLine(remainingLine);
                    }
                }
                else
                    currentPrompt.AppendLine(line);
            }

            var lastUserPrompt = currentPrompt.ToString().Trim();
            if (!string.IsNullOrEmpty(lastUserPrompt))
                result.Add(new AgentConversationStep
                {
                    AgentName = currentAgentName,
                    UserPrompt = lastUserPrompt
                });
        }

        return result;
    }

    private async Task<bool> ValidAgentName(string instanceId, string agentName)
    {
        var agentResourceProvider = _resourceProviderServices[ResourceProviderNames.FoundationaLLM_Agent];

        var nameCheckResult = await agentResourceProvider.ResourceExistsAsync<AgentBase>(
            instanceId,
            agentName,
            _callContext.CurrentUserIdentity!);

        return nameCheckResult.Exists && !nameCheckResult.Deleted;
    }

    #endregion
}
