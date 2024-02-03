using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides an in-memory cache service.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    public class MemoryCacheService(
        ILogger<MemoryCacheService> logger) : ICacheService
    {
        private readonly ILogger _logger = logger;
        private readonly ConcurrentDictionary<CacheKey, CacheItem> _cache = new();

        /// <inheritdoc/>
        public T? Get<T>(CacheKey key)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
                    if (!cacheItem.IsExpired)
                        return (T?)cacheItem.Value;

            return default;
        }

        /// <inheritdoc/>
        public async Task<T?> Get<T>(CacheKey key, Func<Task<T>> valueRetriever, bool allowNull, TimeSpan? expirationTime)
        {
            if (_cache.TryGetValue(key, out var cacheItem))
                if (!cacheItem.IsExpired)
                    return (T?)cacheItem.Value;

            // Attempt to retrieve the cached item using the specified value retriever
            try
            {
                T item = await valueRetriever();
                if ((item != null) || allowNull)
                {
                    _cache[key] = new CacheItem
                    {
                        Value = item,
                        ExpirationTimeUtc = expirationTime.HasValue ? DateTime.UtcNow.Add(expirationTime.Value) : null
                    };
                }
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The value retriever failed for {ItemName}.", key.Name);
                return default;
            }
        }

        /// <inheritdoc/>
        public void Set<T>(CacheKey key, T? value, TimeSpan? expirationTime) =>
            _cache[key] = new CacheItem
            {
                Value = value,
                ExpirationTimeUtc = expirationTime.HasValue ? DateTime.UtcNow.Add(expirationTime.Value) : null
            };

        /// <inheritdoc/>
        public void Remove(CacheKey key) =>
            _cache.TryRemove(key, out _);

        /// <inheritdoc/>
        public void RemoveByCategory(CacheKey key)
        {
            foreach (var cacheKey in _cache.Where(k => k.Key.Category == key.Category).Select(k => k.Key))
                _cache.TryRemove(cacheKey, out _);
        }

        /// <inheritdoc/>
        public int GetItemsCount(string categoryName) =>
            _cache.Where(x => x.Key.Category == categoryName).Count();
    }
}
