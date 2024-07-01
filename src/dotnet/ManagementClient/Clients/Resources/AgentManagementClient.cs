using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class AgentManagementClient(IManagementRESTClient managementRestClient) : IAgentManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                ResourceProviderNames.FoundationaLLM_Agent,
                AgentResourceTypeNames.Agents
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<AgentBase>> GetAgentAsync(string agentName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"Agent '{agentName}' not found.");
            }

            var agent = result[0];

            return agent;
        }

        /// <inheritdoc/>
        public async Task<ResourceNameCheckResult> CheckAgentNameAsync(ResourceName resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
            {
                throw new ArgumentException("Resource name and type must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{AgentResourceProviderActions.CheckName}",
                resourceName
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeAgentAsync(string agentName)
        {
            if (string.IsNullOrWhiteSpace(agentName))
            {
                throw new ArgumentException("The Agent name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}/{AgentResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertAgentAsync(AgentBase agent) => await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Agent,
              $"{AgentResourceTypeNames.Agents}/{agent.Name}",
                agent
            );

        /// <inheritdoc/>
        public async Task DeleteAgentAsync(string agentName) => await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}"
            );
    }
}
