using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Common.Services.Cache
{
    /// <summary>
    /// Provides the caching services used by the FoundationaLLM authorization service client.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used to log information.</param>
    public class AuthorizationServiceClientMemoryCacheService(
        ILogger<AuthorizationServiceClientMemoryCacheService> logger) : IAuthorizationServiceClientCacheService
    {
        private readonly ILogger _logger = logger;

        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 10000, // Limit cache size to 10000 resources.
            ExpirationScanFrequency = TimeSpan.FromSeconds(30) // Scan for expired items every thirty seconds.
        });

        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
           .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache entries are valid for 5 minutes.
           .SetSlidingExpiration(TimeSpan.FromMinutes(2)) // Reset expiration time if accessed within 2 minutes.
           .SetSize(1); // Each cache entry is a single authorization result.

        private readonly SemaphoreSlim _cacheLock = new(1, 1);

        /// <inheritdoc/>
        public async void SetValue(string cacheKey, ActionAuthorizationResult result)
        {
            await _cacheLock.WaitAsync();
            try
            {
                _cache.Set(cacheKey, result, _cacheEntryOptions);
                _logger.LogInformation("The authorization result has been set in the cache.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error setting the authorization result in the cache.");
            }           
            finally
            {
                _cacheLock.Release();
            }
        }

        /// <inheritdoc/>
        public bool TryGetValue(string cacheKey, out ActionAuthorizationResult? result)
        {
            result = default;
            try
            {
                if (_cache.TryGetValue(cacheKey, out ActionAuthorizationResult? cachedValue)
                    && cachedValue != null)
                {
                    result = cachedValue;
                    _logger.LogInformation("Cache hit for the authorization request");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error getting the ActionAuthorizationResult from the cache.");
            }
            return false;
        }

        /// <inheritdoc/>
        public string GenerateCacheKey(
            string instanceId,
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoleAssignments,
            bool includeActions,
            UnifiedUserIdentity userIdentity)
        {
            var sortedResourcePaths = string.Join(",", resourcePaths.Distinct().OrderBy(rp => rp));

            var keyString = $"{instanceId}:{action}:{sortedResourcePaths}:{expandResourceTypePaths}:{includeRoleAssignments}:{includeActions}:{userIdentity.UserId}:{userIdentity.UPN}:{string.Join(",", userIdentity.GroupIds)}";

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
            return Convert.ToBase64String(hashBytes);
        }        
    }
}
