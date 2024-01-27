namespace FoundationaLLM.Management.Interfaces
{
    /// <summary>
    /// Provides cache management functionality.
    /// </summary>
    public interface ICacheManagementService
    {
        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and AgentHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearAgentCache();

        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearDataSourceCache();

        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        Task<bool> ClearPromptCache();
    }
}
