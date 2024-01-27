using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Management.Services.APIServices
{
    /// <summary>
    /// Provides access to the Agent Factory API.
    /// </summary>
    /// <remarks>
    /// Constructor for the Agent Factory APIS client.
    /// </remarks>
    /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
    /// <param name="logger">The logging interface used to log under the
    /// <see cref="AgentFactoryAPIService"/> type name.</param>
    public class AgentFactoryAPIService(IHttpClientFactoryService httpClientFactoryService, ILogger<AgentFactoryAPIService> logger) : IAgentFactoryAPIService
    {
        /// <inheritdoc/>
        public async Task<bool> RemoveCacheItem(string name)
        {
            var client = httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPI);

            var responseMessage = await client.PostAsync($"cache/item/{name}/remove",
                null);

            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation($"Successfully removed cache item: {name}.");
                return true;
            }

            logger.LogWarning($"Failed to remove cache item: {name}. Status code: {responseMessage.StatusCode}");
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveCacheCategory(string name)
        {
            var client = httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPI);

            var responseMessage = await client.PostAsync($"cache/category/{name}/remove",
                null);

            if (responseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation($"Successfully removed cache category: {name}.");
                return true;
            }

            logger.LogWarning($"Failed to remove cache category: {name}. Status code: {responseMessage.StatusCode}");
            return false;
        }

    }
}
