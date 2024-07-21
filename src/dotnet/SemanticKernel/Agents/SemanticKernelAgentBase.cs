using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// Implements the base functionality for a Semantic Kernel agent.
    /// </summary>
    /// <param name="request">The <see cref="LLMCompletionRequest"/> being processed by the agent.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class SemanticKernelAgentBase(
        LLMCompletionRequest request,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger logger)
    {
        protected readonly IResourceProviderService _configurationResourceProvider =
            resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Configuration);

        protected readonly LLMCompletionRequest _request = request;
        protected readonly ILogger _logger = logger;

        protected string _llmProvider = string.Empty;
        protected string _endpoint = string.Empty;
        protected string _deploymentName = string.Empty;

        /// <summary>
        /// Gets the completion for the request.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        public async Task<LLMCompletionResponse> GetCompletion()
        {
            ValidateRequest();
            await ExpandAndValidateAgent();

            return _llmProvider switch
            {
                LanguageModelProviders.AZUREML => await BuildResponseWithAzureML(),
                LanguageModelProviders.MICROSOFT => await BuildResponseWithAzureOpenAI(),
                LanguageModelProviders.OPENAI => await BuildResponseWithOpenAI(),
                _ => throw new SemanticKernelException($"The LLM provider '{_llmProvider}' is not supported.")
            };
        }

        /// <summary>
        /// Builds the completion response using Azure OpenAI.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<LLMCompletionResponse> BuildResponseWithAzureOpenAI()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the completion response using an Azure ML deployed model.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<LLMCompletionResponse> BuildResponseWithAzureML()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the completion response using OpenAI.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<LLMCompletionResponse> BuildResponseWithOpenAI()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the agent-specific details from the agent properties payload.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ExpandAndValidateAgent()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        protected async Task<string> GetConfigurationValue(string configName) =>
            (await GetResource<AppConfigurationKeyBase>(
                configName,
                ConfigurationResourceTypeNames.AppConfigurations,
                _configurationResourceProvider)).Value!;

        private async Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService)
            where T : ResourceBase
        {
            var result = await resourceProviderService.HandleGetAsync(
                $"/{resourceTypeName}/{objectId.Split("/").Last()}", new UnifiedUserIdentity
                {
                    Name = "SemanticKernelAPI",
                    UserId = "SemanticKernelAPI",
                    Username = "SemanticKernelAPI"
                });
            var resource = (result as List<ResourceProviderGetResult<T>>)!.First().Resource;
            return resource;
        }

        private void ValidateRequest()
        {
            if (_request.Agent == null)
                throw new SemanticKernelException("The Agent property of the completion request cannot be null.", StatusCodes.Status400BadRequest);

            if (_request.Agent.OrchestrationSettings == null)
                throw new SemanticKernelException("The OrchestrationSettings property of the agent cannot be null.", StatusCodes.Status400BadRequest);

            if (_request.Agent.OrchestrationSettings.EndpointConfiguration == null)
                throw new SemanticKernelException("The EndpointConfiguration property of the agent's OrchestrationSettings property cannot be null.", StatusCodes.Status400BadRequest);

            if (!_request.Agent.OrchestrationSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.Provider, out var llmProvider)
                || string.IsNullOrWhiteSpace(llmProvider.ToString()))
                throw new SemanticKernelException("The Provider property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", StatusCodes.Status400BadRequest);

            if (!LanguageModelProviders.All.Contains(llmProvider.ToString()))
                throw new SemanticKernelException($"The LLM provider '{llmProvider}' is not supported.", StatusCodes.Status400BadRequest);

            if (!_request.Agent.OrchestrationSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.Endpoint, out var endpoint)
                || string.IsNullOrWhiteSpace(endpoint.ToString()))
                throw new SemanticKernelException("The Endpoint property of the agent's OrchestrationSettings.EndpointConfiguration property cannot be null.", StatusCodes.Status400BadRequest);

            if (_request.Agent.OrchestrationSettings.ModelParameters == null)
                throw new SemanticKernelException("The ModelParameters property of the agent's OrchestrationSettings property cannot be null.", StatusCodes.Status400BadRequest);

            if (!_request.Agent.OrchestrationSettings.ModelParameters.TryGetValue(ModelParameterKeys.DeploymentName, out var deploymentName)
                || string.IsNullOrWhiteSpace(deploymentName.ToString()))
                throw new SemanticKernelException("The DeploymentName property of the agent's OrchestrationSettings.ModelParameters property cannot be null.", StatusCodes.Status400BadRequest);

            _llmProvider = llmProvider.ToString()!;
            _endpoint = endpoint.ToString()!;
            _deploymentName = deploymentName.ToString()!;
        }
    }
}
