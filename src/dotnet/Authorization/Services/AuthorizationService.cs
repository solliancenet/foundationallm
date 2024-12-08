using FoundationaLLM.Authorization.Models.Configuration;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Provides methods for interacting with the Authorization API.
    /// </summary>
    public class AuthorizationService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServiceSettings> options,
        ILogger<AuthorizationService> logger)
        : IAuthorizationService
    {
        private readonly AuthorizationServiceSettings _settings = options.Value;
        private readonly IMemoryCache _authorizationCache = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromSeconds(30), // Scan for expired items every thirty seconds.
        });
        private readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache entries are valid for 5 minutes.
            .SetSlidingExpiration(TimeSpan.FromMinutes(2)) // Reset expiration time if accessed within 2 minutes.
            .SetSize(1); // Each cache entry is a single authorization result.

        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public async Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoleAssignments,
            bool includeActions,
            UnifiedUserIdentity userIdentity)
        {
            var defaultResults = resourcePaths.Distinct().ToDictionary(
                rp => rp,
                rp => new ResourcePathAuthorizationResult
                {
                    ResourceName = string.Empty,
                    ResourcePath = rp,
                    Authorized = false
                });

            try
            {
                var cacheKey = GenerateCacheKey(
                    instanceId,
                    action,
                    resourcePaths,
                    expandResourceTypePaths,
                    includeRoleAssignments,
                    includeActions,
                    userIdentity
                );

                await _cacheLock.WaitAsync();
                try
                {
                    if (_authorizationCache.TryGetValue(cacheKey, out ActionAuthorizationResult? cachedResult))
                    {
                        return cachedResult!;
                    }

                    var authorizationRequest = new ActionAuthorizationRequest
                    {
                        Action = action,
                        ResourcePaths = resourcePaths,
                        ExpandResourceTypePaths = expandResourceTypePaths,
                        IncludeRoles = includeRoleAssignments,
                        IncludeActions = includeActions,
                        UserContext = new UserAuthorizationContext
                        {
                            SecurityPrincipalId = userIdentity.UserId!,
                            UserPrincipalName = userIdentity.UPN!,
                            SecurityGroupIds = userIdentity.GroupIds
                        }
                    };

                    var httpClient = await CreateHttpClient();
                    var response = await httpClient.PostAsync(
                        $"/instances/{instanceId}/authorize",
                        JsonContent.Create(authorizationRequest));

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContentString = await response.Content.ReadAsStringAsync();
                        var responseContent = JsonSerializer.Deserialize<ActionAuthorizationResult>(responseContentString)!;

                        // Add the authorization response to the cache.
                        _authorizationCache.Set(cacheKey, responseContent, _cacheEntryOptions);
                        return responseContent;
                    }

                    logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.",
                        response.StatusCode, response.ReasonPhrase);
                    return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
                }
                finally
                {
                    _cacheLock.Release();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error calling the Authorization API");
                return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
            }
        }


        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments",
                    JsonContent.Create(roleAssignmentRequest));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentOperationResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentOperationResult() { Success = false };

                    return result;
                }

                logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentOperationResult() { Success = false };
            }
        }

        /// <inheritdoc/>
        public async Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/roleassignments/query",
                    JsonContent.Create(queryParameters));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<RoleAssignment>>(responseContent)!;
                }

                logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return [];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error calling the Authorization API");
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.DeleteAsync(
                    $"/instances/{instanceId}/roleassignments/{roleAssignment}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<RoleAssignmentOperationResult>(responseContent);

                    if (result == null)
                        return new RoleAssignmentOperationResult() { Success = false };

                    return result;
                }

                logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentOperationResult() { Success = false };
            }
        }

        /// <summary>
        /// Exception to the unified HTTP client factory when consuming the Authorization API.
        /// </summary>
        /// <returns></returns>
        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_settings.APIUrl);

            var credentials = DefaultAuthentication.AzureCredential;
            var tokenResult = await credentials.GetTokenAsync(
                new([_settings.APIScope]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }

        private string GenerateCacheKey(
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
