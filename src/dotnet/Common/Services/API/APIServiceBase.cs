using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Services.API
{
    /// <summary>
    /// Contains base functionality for calling the Hub APIs.
    /// </summary>
    /// <param name="httpClientName">The name of the HttpClient associated
    /// with the Hub API service that implements this base class.</param>
    /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
    /// <param name="logger">The logging interface.</param>
    public class APIServiceBase(
        string httpClientName,
        IHttpClientFactoryService httpClientFactoryService,
        ILogger logger) : ICacheControlAPIService
    {
        /// <summary>
        /// Store the name of the HttpClient associated with the Hub API service that
        /// is set by the derived class. This is stored within this class-level variable
        /// so the methods in this class can access it.
        /// </summary>
        private readonly string _httpClientName = httpClientName;
        
        /// <summary>
        /// Refreshes the configuration cache.
        /// </summary>
        /// <returns></returns>
        public async Task<APICacheRefreshResult> RefreshConfigurationCache() => await RefreshCache("configuration");

        /// <summary>
        /// Refreshes the named cache.
        /// </summary>
        /// <param name="name">The name of the cache item to refresh.</param>
        /// <returns></returns>
        public async Task<APICacheRefreshResult> RefreshCache(string name)
        {
            var client = httpClientFactoryService.CreateClient(_httpClientName);

            var responseMessage = await client.PostAsync($"manage/cache/{name}/refresh",
                null);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APICacheRefreshResult>(responseContent);
                if (result != null)
                {
                    result.Success = true;
                    logger.LogInformation($"Successfully removed cache item: {name}. API message: {result?.Detail}");
                    return result!;
                }
                else
                {
                    return new APICacheRefreshResult
                    {
                        Success = true,
                        Detail = $"Successfully removed cache item: {name}"
                    };
                }
            }

            var message = $"Failed to remove cache item: {name}. Status code: {responseMessage.StatusCode}";
            logger.LogWarning(message);
            return new APICacheRefreshResult
            {
                Detail = message,
                Success = false
            };
        }
    }
}
