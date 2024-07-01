using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage agent resources.
    /// </summary>
    public interface IAgentManagementClient
    {
        /// <summary>
        /// Retrieves all agent resources.
        /// </summary>
        /// <returns>All agent resources to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync();

        /// <summary>
        /// Retrieves a specific agent by name.
        /// </summary>
        /// <param name="agentName">The name of the agent resource to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<AgentBase>> GetAgentAsync(string agentName);

        /// <summary>
        /// Checks the availability of a resource name for an agent. If the name is available, the
        /// <see cref="ResourceNameCheckResult.Status"/> value will be "Allowed". If the name is
        /// not available, the <see cref="ResourceNameCheckResult.Status"/> value will be "Denied" and
        /// the <see cref="ResourceNameCheckResult.Message"/> will explain the reason why. Typically,
        /// a denied name is due to a name conflict with an existing agent or an agent that was
        /// deleted but not purged.
        /// </summary>
        /// <param name="resourceName">Contains the name of the resource to check.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the required properties within the argument
        /// are empty or missing.</exception>
        Task<ResourceNameCheckResult> CheckAgentNameAsync(ResourceName resourceName);

        /// <summary>
        /// Purges a deleted agent by its name. This action is irreversible.
        /// </summary>
        /// <param name="agentName">The name of the agent to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeAgentAsync(string agentName);

        /// <summary>
        /// Upserts an agent resource. If an agent does not exist, it will be created. If an agent
        /// does exist, it will be updated.
        /// </summary>
        /// <param name="agent">The agent resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertAgentAsync(AgentBase agent);

        /// <summary>
        /// Deletes an agent resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgeAgentAsync"/> method with the same name.
        /// </summary>
        /// <param name="agentName">The name of the agent resource to delete.</param>
        /// <returns></returns>
        Task DeleteAgentAsync(string agentName);
    }
}
