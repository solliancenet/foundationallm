using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides the caching services used by the FoundationaLLM authorization service client.
    /// </summary>
    public interface IAuthorizationServiceClientCacheService
    {
        /// <summary>
        /// Attempts to retrieve an <see cref="ActionAuthorizationResult"/> from the cache.
        /// </summary>
        /// <param name="cacheKey">The key used to identify the resource value in the cache.</param>
        /// <param name="result">The <see cref="ActionAuthorizationResult"/> to be retrieved.</param>
        /// <returns><see langword="true"/> if the value was found in the cache, <see langword="false"/> otherwise.</returns>
        bool TryGetValue(string cacheKey, out ActionAuthorizationResult? result);

        /// <summary>
        /// Sets an <see cref="ActionAuthorizationResult"/> in the cache.
        /// </summary>       
        /// <param name="cacheKey">The key used to identify the resource value in the cache.</param>
        /// <param name="result">The <see cref="ActionAuthorizationResult"/> value to be set in the cache.</param>
        void SetValue(string cacheKey, ActionAuthorizationResult result);

        /// <summary>
        /// Generates a cache key based on the provided parameters representing an authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance idenitifier.</param>
        /// <param name="action">The action that was authorized.</param>
        /// <param name="resourcePaths">The resource path(s) being authorized.</param>
        /// <param name="expandResourceTypePaths">Setting to deterime if resource type paths should be expanded.</param>
        /// <param name="includeRoleAssignments">Setting to determine if role assignments are included as part of the authorization.</param>
        /// <param name="includeActions">Setting to determine if actions are included as part of the authorization.</param>
        /// <param name="userIdentity">The identity of the process initiating the authorization process.</param>
        /// <returns>The generated cache key for the authorization built from the parameters passed in.</returns>
        string GenerateCacheKey(
            string instanceId,
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoleAssignments,
            bool includeActions,
            UnifiedUserIdentity userIdentity);
    }
}
