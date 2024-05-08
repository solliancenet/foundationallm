using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Catalogs;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Options;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class ManagementAPITestManager(
        IHttpClientManager httpClientManager,
        IOptions<InstanceSettings> instanceSettings) : IManagementAPITestManager
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<AgentBase> CreateAgent(string agentName)
        {
            // All test agents should have a corresponding prompt in the catalog.
            // Retrieve the agent and prompt from the test catalog.
            var agent = AgentCatalog.GetAllAgents().FirstOrDefault(a => a.Name == agentName);
            var prompt = PromptCatalog.GetAllPrompts().FirstOrDefault(p => p.Name == agentName);

            if (agent == null)
            {
                throw new InvalidOperationException($"The agent {agentName} was not found.");
            }
            if (prompt == null)
            {
                throw new InvalidOperationException($"The prompt for the agent {agentName} was not found.");
            }

            // Resolve App Config values for the endpoint configuration as necessary.
            // Note: This is a temporary workaround until we have the Models and Endpoints resource provider in place.
            //if (agent.OrchestrationSettings is { EndpointConfiguration: not null })
            //{
            //    foreach (var (key, value) in agent.OrchestrationSettings.EndpointConfiguration)
            //    {
            //        if (key.ToLower() == "api_key") continue;
            //        if (value is not string stringValue || !stringValue.StartsWith("FoundationaLLM:")) continue;
            //        var appConfigValue = await TestConfiguration.GetAppConfigValueAsync(value.ToString()!);
            //        agent.OrchestrationSettings.EndpointConfiguration[key] = appConfigValue;
            //    }
            //}

            // Create the prompt for the agent.
            var promptObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"prompts/{agentName}",
                prompt);

            // Add the prompt ObjectId to the agent.
            agent.PromptObjectId = promptObjectId;

            // TODO: Create any other dependencies for the agent here.

            // Create the agent.
            var agentObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Agent,
                $"agents/{agentName}",
                agent);

            agent.ObjectId = agentObjectId;

            return agent;
        }

        /// <inheritdoc/>
        public async Task DeleteAgent(string agentName)
        {
            // Delete the agent and its dependencies.

            // Delete the agent's prompt.
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"prompts/{agentName}");

            // TODO: Delete other dependencies for the agent here.

            // Delete the agent.
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Agent,
                $"agents/{agentName}");
        }

        /// <inheritdoc/>
        public async Task<object?> GetResourcesAsync(string instanceId, string resourceProvider, string resourcePath)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var response = await coreClient.GetAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var resources = JsonSerializer.Deserialize<object>(responseContent, _jsonSerializerOptions);
                return resources;
            }

            throw new FoundationaLLMException($"Failed to get resources. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync(string instanceId, string resourceProvider, string resourcePath,
            object resource)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var serializedRequest = JsonSerializer.Serialize(resource, _jsonSerializerOptions);

            var response = await coreClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                               new StringContent(serializedRequest, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var upsertResult = JsonSerializer.Deserialize<ResourceProviderUpsertResult>(responseContent, _jsonSerializerOptions);
                if (upsertResult != null)
                    return upsertResult.ObjectId ??
                           throw new InvalidOperationException("The returned object ID is invalid.");
            }

            throw new FoundationaLLMException($"Failed to upsert resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task DeleteResourceAsync(string instanceId, string resourceProvider, string resourcePath)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var response = await coreClient.DeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");

            if (response.IsSuccessStatusCode)
            {
                // Resource was deleted successfully. Now purge the resource, so we can reuse the name.
                await coreClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}/purge",
                    new StringContent("{}", Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new FoundationaLLMException($"Successfully deleted the resource, but failed to purge it. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
                return;
            }

            throw new FoundationaLLMException($"Failed to delete resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }
    }
}