using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Contains base functionality for calling the underlying API service.
    /// </summary>
    public interface ICacheControlAPIService
    {
        /// <summary>
        /// Refreshes the configuration cache.
        /// </summary>
        /// <returns></returns>
        Task<APICacheRefreshResult> RefreshConfigurationCache();

        /// <summary>
        /// Refreshes the named cache.
        /// </summary>
        /// <param name="name">The name of the cache item to refresh.</param>
        /// <returns></returns>
        Task<APICacheRefreshResult> RefreshCache(string name);
    }
}
