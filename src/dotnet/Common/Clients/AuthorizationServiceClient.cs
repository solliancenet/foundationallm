using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods for interacting with the Authorization API.
    /// </summary>
    public class AuthorizationServiceClient : IAuthorizationServiceClient
    {
        private readonly AuthorizationServiceClientSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthorizationServiceClientCacheService _cacheService;
        private readonly ILogger<AuthorizationServiceClient> _logger;        

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServiceClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The centralized factory from which to create HTTP clients.</param>     
        /// <param name="options"><see cref="AuthorizationServiceClientSettings"/> options.</param>
        /// <param name="cacheService">The cache service used to store authorization results for quick retrieval.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AuthorizationServiceClient(
            IHttpClientFactory httpClientFactory,
            IOptions<AuthorizationServiceClientSettings> options,
            IAuthorizationServiceClientCacheService cacheService,
            ILogger<AuthorizationServiceClient> logger)
        {
            _settings = options.Value;
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
            _logger = logger;
        }

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
                string cacheKey = string.Empty; 

                if (_settings.EnableCache)
                {
                    cacheKey = _cacheService.GenerateCacheKey(
                                       instanceId,
                                       action,
                                       resourcePaths,
                                       expandResourceTypePaths,
                                       includeRoleAssignments,
                                       includeActions,
                                       userIdentity);

                    _cacheService.TryGetValue(cacheKey, out ActionAuthorizationResult? cachedResult);

                    if (cachedResult != null)
                    {
                        return cachedResult;
                    }                        
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
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ActionAuthorizationResult>(responseContent);
                    if (result != null)
                    {
                        if (_settings.EnableCache)
                        {
                            _cacheService.SetValue(cacheKey, result);
                        }
                        
                        return result;
                    }
                    _logger.LogError("The response from the Authorization API was invalid and could not be parsed.");
                    return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
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

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
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

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
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

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new RoleAssignmentOperationResult() { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new RoleAssignmentOperationResult() { Success = false };
            }
        }

        #region Manage secret keys

        /// <inheritdoc/>
        public async Task<List<SecretKey>> GetSecretKeys(string instanceId, string contextId)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.GetAsync(
                    $"/instances/{instanceId}/secretkeys/{contextId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<SecretKey>>(responseContent)!;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<string?> UpsertSecretKey(string instanceId, SecretKey secretKey)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/secretkeys",
                    JsonContent.Create(secretKey));

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<string?>(responseContent);
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteSecretKey(string instanceId, string contextId, string secretKeyId)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.DeleteAsync(
                    $"/instances/{instanceId}/secretkeys/{contextId}?secretKeyId={secretKeyId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
            }
        }

        /// <inheritdoc/>
        public async Task<SecretKeyValidationResult> ValidateSecretKey(string instanceId, string contextId, string secretKeyValue)
        {
            try
            {
                var httpClient = await CreateHttpClient();
                var response = await httpClient.PostAsync(
                    $"/instances/{instanceId}/secretkeys/{contextId}?secretKeyValue={secretKeyValue}", null);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<SecretKeyValidationResult>(responseContent)!;
                }

                _logger.LogError("The call to the Authorization API returned an error: {StatusCode} - {ReasonPhrase}.", response.StatusCode, response.ReasonPhrase);
                return new SecretKeyValidationResult() { Valid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error calling the Authorization API");
                return new SecretKeyValidationResult() { Valid = false };
            }
        }

        #endregion

        /// <summary>
        /// Exception to the unified HTTP client factory when consuming the Authorization API.
        /// </summary>
        /// <returns></returns>
        private async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_settings.APIUrl);

            var credentials = DefaultAuthentication.AzureCredential;
            var tokenResult = await credentials!.GetTokenAsync(
                new([_settings.APIScope]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }
    }
}
