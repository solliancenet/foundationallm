using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Management.Interfaces;

namespace FoundationaLLM.Management.Services
{
    /// <summary>
    /// Provides cache management functionality.
    /// </summary>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="CacheManagementService"/> type name.</param>
    /// <param name="agentFactoryApiService">Provides functionality for calling the
    /// AgentFactoryAPI.</param>
    /// <param name="agentHubApiService">Provides functionality for calling the
    /// AgentHubAPI.</param>
    /// <param name="dataSourceHubApiService">Provides functionality for calling the
    /// DataSourceHubAPI.</param>
    /// <param name="promptHubApiService">Provides functionality for calling the
    /// PromptHubAPI.</param>
    public class CacheManagementService(
        ILogger<CacheManagementService> logger,
        IAgentFactoryAPIService agentFactoryApiService,
        IAgentHubAPIService agentHubApiService,
        IDataSourceHubAPIService dataSourceHubApiService,
        IPromptHubAPIService promptHubApiService) : ICacheManagementService
    {
        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and AgentHubService.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearAgentCache()
        {
            try
            {
                var removeAgentCacheTask = agentFactoryApiService.RemoveCacheCategory(CacheCategories.Agent);
                var refreshConfigurationCacheTask = agentHubApiService.RefreshConfigurationCache();

                // Run all tasks concurrently and wait for them to complete.
                await Task.WhenAll(removeAgentCacheTask, refreshConfigurationCacheTask);

                var isAgentCacheRemoved = await removeAgentCacheTask;
                var isConfigurationCacheRefreshed = await refreshConfigurationCacheTask;

                if (isAgentCacheRemoved && isConfigurationCacheRefreshed.Success)
                {
                    return true;
                }

                logger.LogError("One or more tasks failed in ClearAgentCache.");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clear the agent cache.");
                return false;
            }
        }

        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearDataSourceCache()
        {
            try
            {
                var removeDataSourceCacheTask = agentFactoryApiService.RemoveCacheCategory(CacheCategories.DataSource);
                var refreshConfigurationCacheTask = dataSourceHubApiService.RefreshConfigurationCache();

                // Run all tasks concurrently and wait for them to complete.
                await Task.WhenAll(removeDataSourceCacheTask, refreshConfigurationCacheTask);

                var isDataSourceCacheRemoved = await removeDataSourceCacheTask;
                var isConfigurationCacheRefreshed = await refreshConfigurationCacheTask;

                if (isDataSourceCacheRemoved && isConfigurationCacheRefreshed.Success)
                {
                    return true;
                }

                logger.LogError("One or more tasks failed in ClearDataSourceCache.");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clear the data source cache.");
                return false;
            }
        }

        /// <summary>
        /// Clears the agent cache from the AgentFactoryService and DataSourceHubService.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearPromptCache()
        {
            try
            {
                var removePromptCacheTask = agentFactoryApiService.RemoveCacheCategory(CacheCategories.Prompt);
                var refreshConfigurationCacheTask = promptHubApiService.RefreshConfigurationCache();

                // Run all tasks concurrently and wait for them to complete.
                await Task.WhenAll(removePromptCacheTask, refreshConfigurationCacheTask);

                var isPromptCacheRemoved = await removePromptCacheTask;
                var isConfigurationCacheRefreshed = await refreshConfigurationCacheTask;

                if (isPromptCacheRemoved && isConfigurationCacheRefreshed.Success)
                {
                    return true;
                }

                logger.LogError("One or more tasks failed in ClearPromptCache.");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clear the prompt cache.");
                return false;
            }
        }
    }
}
