using FluentValidation;
using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.AuthorizationEngine.Models;
using FoundationaLLM.AuthorizationEngine.Models.Configuration;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Utils;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Constants.Authentication;

namespace FoundationaLLM.AuthorizationEngine.Services
{
    /// <summary>
    /// Implements the core authorization engine.
    /// </summary>
    public class AuthorizationCore : IAuthorizationCore
    {
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IStorageService _storageService;
        private readonly IConfiguration _configuration;
        private readonly IResourceValidatorFactory _resourceValidatorFactory;
        private readonly ILogger<AuthorizationCore> _logger;
        private readonly AuthorizationCoreSettings _settings;
        private readonly ConcurrentDictionary<string, RoleAssignmentStore> _roleAssignmentStores = [];
        private readonly ConcurrentDictionary<string, PolicyAssignmentStore> _policyAssignmentStores = [];
        private readonly ConcurrentDictionary<string, SecretKeyStore> _secretKeyStores = [];
        private readonly ConcurrentDictionary<string, RoleAssignmentCache> _roleAssignmentCaches = [];
        private readonly ConcurrentDictionary<string, PolicyAssignmentCache> _policyAssignmentCaches = [];
        private readonly ConcurrentDictionary<string, SecretKeyCache> _secretKeyCaches = [];
        private readonly IValidator<ActionAuthorizationRequest> _actionAuthorizationRequestValidator;

        private const string ROLE_ASSIGNMENTS_CONTAINER_NAME = "role-assignments";
        private const string POLICY_ASSIGNMENTS_CONTAINER_NAME = "policy-assignments";
        private const string SECRET_KEYS_CONTAINER_NAME = "secret-keys";

        private bool _initialized = false;
        private readonly SemaphoreSlim _syncRoot = new(1, 1);

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationCore"/> class.
        /// </summary>
        /// <param name="options">The options used to configure the authorization core.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
        /// <param name="azureKeyVaultService">The <see cref="IAzureKeyVaultService"/> providing key vault services.</param>
        /// <param name="configuration">The application configuration values.</param>
        /// <param name="resourceValidatorFactory"> The resource validator factory used to create resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AuthorizationCore(
            IOptions<AuthorizationCoreSettings> options,
            IStorageService storageService,
            IAzureKeyVaultService azureKeyVaultService,
            IConfiguration configuration,
            IResourceValidatorFactory resourceValidatorFactory,
            ILogger<AuthorizationCore> logger)
        {
            _settings = options.Value;
            _storageService = storageService;
            _azureKeyVaultService = azureKeyVaultService;
            _configuration = configuration;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;

            _actionAuthorizationRequestValidator = _resourceValidatorFactory.GetValidator<ActionAuthorizationRequest>()!;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _initialized property.
            _ = Task.Run(Initialize);
        }

        private async Task Initialize()
        {
            try
            {
                foreach (var instanceId in _settings.InstanceIds)
                {
                    var roleAssignmentStoreFile = $"/{instanceId.ToLower()}.json";
                    var policyAssignmentStoreFile = $"/{instanceId.ToLower()}-policy.json";
                    var secretKeysStoreFile = $"/{instanceId.ToLower()}-secret-keys.json";
                    RoleAssignmentStore? roleAssignmentStore;
                    PolicyAssignmentStore? policyAssignmentStore;
                    SecretKeyStore? secretKeyStore;

                    #region Load role assignments

                    if (await _storageService.FileExistsAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default))
                    {
                        var fileContent = await _storageService.ReadFileAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default);
                        roleAssignmentStore = JsonSerializer.Deserialize<RoleAssignmentStore>(
                            Encoding.UTF8.GetString(fileContent.ToArray()));
                        if (roleAssignmentStore == null
                            || string.Compare(roleAssignmentStore.InstanceId, instanceId) != 0)
                        {
                            _logger.LogError("The role assignment store file for instance {InstanceId} is invalid.", instanceId);
                        }
                        else
                        {
                            _roleAssignmentStores.AddOrUpdate(instanceId, roleAssignmentStore, (k, v) => roleAssignmentStore);
                            _logger.LogInformation("The role assignment store for instance {InstanceId} has been loaded.", instanceId);
                        }
                    }
                    else
                    {
                        roleAssignmentStore = new RoleAssignmentStore
                        {
                            InstanceId = instanceId,
                            RoleAssignments = []
                        };

                        _roleAssignmentStores.AddOrUpdate(instanceId, roleAssignmentStore, (k, v) => roleAssignmentStore);
                        await _storageService.WriteFileAsync(
                            ROLE_ASSIGNMENTS_CONTAINER_NAME,
                            roleAssignmentStoreFile,
                            JsonSerializer.Serialize(roleAssignmentStore),
                            default,
                            default);
                        _logger.LogInformation("The role assignment store for instance {InstanceId} has been created.", instanceId);
                    }

                    #endregion

                    #region Load policy assignments

                    if (await _storageService.FileExistsAsync(POLICY_ASSIGNMENTS_CONTAINER_NAME, policyAssignmentStoreFile, default))
                    {
                        var fileContent = await _storageService.ReadFileAsync(POLICY_ASSIGNMENTS_CONTAINER_NAME, policyAssignmentStoreFile, default);
                        policyAssignmentStore = JsonSerializer.Deserialize<PolicyAssignmentStore>(
                            Encoding.UTF8.GetString(fileContent.ToArray()));
                        if (policyAssignmentStore == null
                            || string.Compare(policyAssignmentStore.InstanceId, instanceId) != 0)
                        {
                            _logger.LogError("The policy assignment store file for instance {InstanceId} is invalid.", instanceId);
                        }
                        else
                        {
                            _policyAssignmentStores.AddOrUpdate(instanceId, policyAssignmentStore, (k, v) => policyAssignmentStore);
                            _logger.LogInformation("The policy assignment store for instance {InstanceId} has been loaded.", instanceId);
                        }
                    }
                    else
                    {
                        policyAssignmentStore = new PolicyAssignmentStore
                        {
                            InstanceId = instanceId,
                            PolicyAssignments = []
                        };

                        _policyAssignmentStores.AddOrUpdate(instanceId, policyAssignmentStore, (k, v) => policyAssignmentStore);
                        await _storageService.WriteFileAsync(
                            POLICY_ASSIGNMENTS_CONTAINER_NAME,
                            policyAssignmentStoreFile,
                            JsonSerializer.Serialize(policyAssignmentStore),
                            default,
                            default);
                        _logger.LogInformation("The policy assignment store for instance {InstanceId} has been created.", instanceId);
                    }

                    #endregion

                    #region Load secret keys

                    if (await _storageService.FileExistsAsync(SECRET_KEYS_CONTAINER_NAME, secretKeysStoreFile, default))
                    {
                        var fileContent = await _storageService.ReadFileAsync(SECRET_KEYS_CONTAINER_NAME, secretKeysStoreFile, default);
                        secretKeyStore = JsonSerializer.Deserialize<SecretKeyStore>(
                            Encoding.UTF8.GetString(fileContent.ToArray()));
                        if (secretKeyStore == null
                            || string.Compare(secretKeyStore.InstanceId, instanceId) != 0)
                        {
                            _logger.LogError("The secret key store file for instance {InstanceId} is invalid.", instanceId);
                        }
                        else
                        {
                            _secretKeyStores.AddOrUpdate(instanceId, secretKeyStore, (k, v) => secretKeyStore);
                            _logger.LogInformation("The secret key store for instance {InstanceId} has been loaded.", instanceId);
                        }
                    }
                    else
                    {
                        secretKeyStore = new SecretKeyStore
                        {
                            InstanceId = instanceId,
                            SecretKeys = []
                        };

                        _secretKeyStores.AddOrUpdate(instanceId, secretKeyStore, (k, v) => secretKeyStore);
                        await _storageService.WriteFileAsync(
                            SECRET_KEYS_CONTAINER_NAME,
                            secretKeysStoreFile,
                            JsonSerializer.Serialize(secretKeyStore),
                            default,
                            default);
                        _logger.LogInformation("The secret key store for instance {InstanceId} has been created.", instanceId);
                    }

                    #endregion

                    if (roleAssignmentStore != null)
                    {
                        roleAssignmentStore.EnrichRoleAssignments();
                        _roleAssignmentCaches.AddOrUpdate(instanceId, new RoleAssignmentCache(roleAssignmentStore), (k, v) => v);
                    }

                    if (policyAssignmentStore != null)
                    {
                        policyAssignmentStore.EnrichPolicyAssignments();
                        _policyAssignmentCaches.AddOrUpdate(instanceId, new PolicyAssignmentCache(policyAssignmentStore), (k, v) => v);
                    }

                    if (secretKeyStore != null)
                    {
                        _secretKeyCaches.AddOrUpdate(
                            instanceId,
                            new SecretKeyCache(secretKeyStore, _configuration, _logger),
                            (k, v) => v);
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The authorization core failed to initialize.");
            }
        }

        #region Process authorization requests

        /// <inheritdoc/>
        public bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId)
        {
            var resourcePath = $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}";
            _ = ResourcePath.TryParse(
                resourcePath,
                [ResourceProviderNames.FoundationaLLM_Authorization],
                AuthorizationResourceProviderMetadata.AllowedResourceTypes,
                false,
                out ResourcePath? parsedResourcePath);
            var result = ProcessAuthorizationRequestForResourcePath(parsedResourcePath!, new ActionAuthorizationRequest
            {
                Action = AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Read,
                ResourcePaths = [resourcePath],
                ExpandResourceTypePaths = false,
                IncludeRoles = false,
                IncludeActions = false,
                UserContext = new UserAuthorizationContext
                {
                    SecurityPrincipalId = securityPrincipalId,
                    UserPrincipalName = securityPrincipalId,
                    SecurityGroupIds = []
                }
            });
            return result.Authorized;
        }

        /// <inheritdoc/>
        public ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest)
        {
            var authorizationResults = authorizationRequest.ResourcePaths.Distinct().ToDictionary(rp => rp, rp => new ResourcePathAuthorizationResult
            {
                ResourceName = string.Empty,
                ResourcePath = rp,
                Authorized = false,
                Roles = [],
                PolicyDefinitionIds = [],
                SubordinateResourcePathsAuthorizationResults = []
            });
            var invalidResourcePaths = new List<string>();

            try
            {
                _logger.LogDebug("Authorization request: {AuthorizationRequest}",
                    JsonSerializer.Serialize(authorizationRequest));

                if (!_initialized)
                {
                    _logger.LogError("The authorization core is not initialized.");
                    return new ActionAuthorizationResult { AuthorizationResults = authorizationResults };
                }

                // Basic validation
                _actionAuthorizationRequestValidator.ValidateAndThrow(authorizationRequest);

                foreach (var rp in authorizationRequest.ResourcePaths)
                {
                    try
                    {
                        var parsedResourcePath = ResourcePathUtils.ParseForAuthorizationRequestResourcePath(rp, _settings.InstanceIds);

                        if (string.IsNullOrWhiteSpace(parsedResourcePath.InstanceId)
                            || StringComparer.OrdinalIgnoreCase.Compare(parsedResourcePath.InstanceId, instanceId) != 0)
                        {
                            _logger.LogError("The instance id from the controller route and the instance id from the authorization request do not match.");
                            invalidResourcePaths.Add(rp);
                        }
                        else
                        {
                            authorizationResults[rp] = ProcessAuthorizationRequestForResourcePath(parsedResourcePath, new ActionAuthorizationRequest()
                            {
                                Action = authorizationRequest.Action,
                                ResourcePaths = [rp],
                                ExpandResourceTypePaths = parsedResourcePath.IsResourceTypePath
                                    ? authorizationRequest.ExpandResourceTypePaths
                                    : false,
                                IncludeRoles = authorizationRequest.IncludeRoles,
                                IncludeActions = authorizationRequest.IncludeActions,
                                UserContext = authorizationRequest.UserContext
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // If anything goes wrong, we default to denying the request on that particular resource.
                        _logger.LogWarning(ex, "The authorization core failed to process the authorization request for: {ResourcePath}.", rp);
                        invalidResourcePaths.Add(rp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The authorization core failed to process the authorization request.");
            }

            return new ActionAuthorizationResult
            {
                AuthorizationResults = authorizationResults,
                InvalidResourcePaths = invalidResourcePaths
            };
        }

        #region Helpers for processing authorization requests

        private ResourcePathAuthorizationResult ProcessAuthorizationRequestForResourcePath(
            ResourcePath resourcePath,
            ActionAuthorizationRequest authorizationRequest)
        {
            var result = new ResourcePathAuthorizationResult
            {
                ResourceName = resourcePath.MainResourceId,
                ResourcePath = resourcePath.RawResourcePath,
                Authorized = false,
                Roles = [],
                PolicyDefinitionIds = [],
                SubordinateResourcePathsAuthorizationResults = []
            };

            // Combine the principal id and security group ids into one list.
            var securityPrincipalIds = new List<string> { authorizationRequest.UserContext.SecurityPrincipalId };
            if (authorizationRequest.UserContext.SecurityGroupIds != null)
                securityPrincipalIds.AddRange(authorizationRequest.UserContext.SecurityGroupIds);

            if (_policyAssignmentCaches.TryGetValue(resourcePath.InstanceId!, out var policyAssignmentCache))
            {
                // Policies are only assigned to resource type paths.
                result.PolicyDefinitionIds = policyAssignmentCache
                    .GetPolicyAssignments(resourcePath.GetResourceTypeObjectId())
                    .Where(pa => securityPrincipalIds.Contains(pa.PrincipalId))
                    .Select(pa => pa.PolicyDefinitionId)
                    .ToList();
            }

            // Get cache associated with the instance id.
            if (_roleAssignmentCaches.TryGetValue(resourcePath.InstanceId!, out var roleAssignmentCache))
            {
                List<RoleAssignment> allRoleAssignments = [];
                HashSet<string> allSecurableActions = [];
                foreach (var securityPrincipalId in securityPrincipalIds)
                {
                    // Retrieve all role assignments associated with the security principal id.
                    var roleAssignments = roleAssignmentCache.GetRoleAssignments(securityPrincipalId);
                    foreach (var roleAssignment in roleAssignments)
                    {
                        // Retrieve the role definition object
                        if (RoleDefinitions.All.TryGetValue(roleAssignment.RoleDefinitionId, out var roleDefinition))
                        {
                            // Check if the scope of the role assignment covers the resource.
                            // Check if the actions of the role definition include the requested action.
                            if (resourcePath.IncludesResourcePath(roleAssignment.ScopeResourcePath!)
                                && roleAssignment.AllowedActions.Contains(authorizationRequest.Action))
                            {
                                result.Authorized = true;

                                // If we are not asked to include roles or actions and not asked to expand resource paths,
                                // we can return immediately (this is the most common case).
                                // Otherwise, we need to go through the entire list of security principals and their role assignments,
                                // to include collect all the roles/actions and/or all the subordinate authorized resource paths.
                                if (!authorizationRequest.IncludeRoles
                                    && !authorizationRequest.IncludeActions
                                    && !authorizationRequest.ExpandResourceTypePaths)
                                    return result;

                                allSecurableActions.UnionWith(roleAssignment.AllowedActions);
                            }
                        }
                        else
                            _logger.LogWarning("The role assignment {RoleAssignmentName} references the role definition {RoleDefinitionId} which is invalid.",
                                roleAssignment.Name, roleAssignment.RoleDefinitionId);
                    }

                    allRoleAssignments.AddRange(roleAssignments);
                }

                if (!result.Authorized
                    && !resourcePath.IsResourceTypePath)
                {
                    _logger.LogWarning("The action {ActionName} is not allowed on the resource {ResourcePath} for the principal {PrincipalId}.",
                        authorizationRequest.Action,
                        resourcePath,
                        authorizationRequest.UserContext.SecurityPrincipalId);
                }

                if (authorizationRequest.IncludeRoles
                    && allRoleAssignments.Count > 0)
                {
                    // Include the display names of the roles in the result.
                    result.Roles = allRoleAssignments
                        .Select(ra => ra.RoleDefinition!.DisplayName!)
                        .Distinct()
                        .ToList();
                }

                if (authorizationRequest.IncludeActions
                    && allSecurableActions.Count > 0)
                {
                    // Include the securable actions in the result.
                    result.Actions = [.. allSecurableActions];
                }

                if (authorizationRequest.ExpandResourceTypePaths
                    && resourcePath.IsResourceTypePath)
                {
                    Dictionary<string, ResourcePathAuthorizationResult> subordinateResults = [];

                    // If the resource path is a resource type path, we need to expand the resource type path.
                    // We will check all the resource paths that are authorized and add them to the list of subordinate authorized resource paths.
                    foreach (var roleAssignment in allRoleAssignments)
                    {
                        // Considering only role assignments for resource paths that are subordinate to the requested resource path.
                        if (roleAssignment.ScopeResourcePath!.IncludesResourcePath(resourcePath, allowEqual: false))
                        {
                            // Keep track of all role assignments until the end, when we know for sure whether the action is authorized or not.
                            if (!subordinateResults.ContainsKey(
                                roleAssignment.ScopeResourcePath!.MainResourceId!))
                            {
                                subordinateResults.Add(
                                    roleAssignment.ScopeResourcePath!.MainResourceId!,
                                    new ResourcePathAuthorizationResult
                                    {
                                        ResourceName = roleAssignment.ScopeResourcePath!.MainResourceId,
                                        ResourcePath = roleAssignment.ScopeResourcePath!.RawResourcePath,
                                        Authorized = false,
                                        Roles = [],
                                        Actions = [],
                                        SubordinateResourcePathsAuthorizationResults = []
                                    });
                            }

                            var subordinateResult =
                                subordinateResults[roleAssignment.ScopeResourcePath!.MainResourceId!];

                            if (authorizationRequest.IncludeRoles
                                && !subordinateResult.Roles.Contains(roleAssignment.RoleDefinition!.DisplayName!))
                            {
                                subordinateResult.Roles.Add(roleAssignment.RoleDefinition!.DisplayName!);
                            }

                            if (authorizationRequest.IncludeActions)
                            {
                                subordinateResult.Actions = subordinateResult.Actions.Union(roleAssignment.AllowedActions).ToList();
                            }

                            if (roleAssignment.AllowedActions.Contains(authorizationRequest.Action))
                            {
                                subordinateResult.Authorized = true;
                            }
                        }
                    }

                    result.SubordinateResourcePathsAuthorizationResults =
                        subordinateResults;
                }
            }

            return result;
        }

        #endregion

        #endregion

        #region Role assignments

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> CreateRoleAssignment(string instanceId, RoleAssignmentRequest roleAssignmentRequest)
        {
            var roleAssignmentStoreFile = $"/{instanceId.ToLower()}.json";

            if (await _storageService.FileExistsAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default))
            {
                try
                {
                    await _syncRoot.WaitAsync();

                    var fileContent = await _storageService.ReadFileAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default);
                    var roleAssignmentStore = JsonSerializer.Deserialize<RoleAssignmentStore>(
                        Encoding.UTF8.GetString(fileContent.ToArray()));
                    if (roleAssignmentStore != null)
                    {
                        var exists = roleAssignmentStore.RoleAssignments.Any(x => x.PrincipalId == roleAssignmentRequest.PrincipalId
                                                                               && x.Scope == roleAssignmentRequest.Scope
                                                                               && x.RoleDefinitionId == roleAssignmentRequest.RoleDefinitionId);
                        if (!exists)
                        {
                            var roleAssignment = new RoleAssignment()
                            {
                                Type = $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}",
                                Name = roleAssignmentRequest.Name,
                                Description = roleAssignmentRequest.Description,
                                ObjectId = roleAssignmentRequest.ObjectId,
                                PrincipalId = roleAssignmentRequest.PrincipalId,
                                PrincipalType = roleAssignmentRequest.PrincipalType,
                                RoleDefinitionId = roleAssignmentRequest.RoleDefinitionId,
                                Scope = roleAssignmentRequest.Scope,
                                CreatedBy = roleAssignmentRequest.CreatedBy
                            };

                            roleAssignmentStore.RoleAssignments.Add(roleAssignment);
                            _roleAssignmentStores.AddOrUpdate(instanceId, roleAssignmentStore, (k, v) => roleAssignmentStore);
                            roleAssignmentStore.EnrichRoleAssignments();
                            _roleAssignmentCaches[instanceId].AddOrUpdateRoleAssignment(roleAssignment);

                            await _storageService.WriteFileAsync(
                                    ROLE_ASSIGNMENTS_CONTAINER_NAME,
                                    roleAssignmentStoreFile,
                                    JsonSerializer.Serialize(roleAssignmentStore),
                                    default,
                                    default);

                            return new RoleAssignmentOperationResult() { Success = true };
                        }
                        else
                        {
                            return new RoleAssignmentOperationResult()
                            {
                                Success = false, ResultReason = RoleAssignmentResultReasons.AssignmentExists
                            };
                        }
                    }
                }
                finally
                {
                    _syncRoot.Release();
                }
            }

            return new RoleAssignmentOperationResult() { Success = false };
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> DeleteRoleAssignment(string instanceId, string roleAssignment)
        {
            var existingRoleAssignment = _roleAssignmentStores[instanceId].RoleAssignments
                .SingleOrDefault(x => x.Name == roleAssignment);
            if (existingRoleAssignment != null)
            {
                _roleAssignmentCaches[instanceId].RemoveRoleAssignment(roleAssignment);
                _roleAssignmentStores[instanceId].RoleAssignments.Remove(existingRoleAssignment);

                await _storageService.WriteFileAsync(
                   ROLE_ASSIGNMENTS_CONTAINER_NAME,
                   $"/{instanceId.ToLower()}.json",
                   JsonSerializer.Serialize(_roleAssignmentStores[instanceId]),
                   default,
                   default);

                return new RoleAssignmentOperationResult() { Success = true };
            }

            return new RoleAssignmentOperationResult() { Success = false };
        }

        /// <inheritdoc/>
        public List<RoleAssignment> GetRoleAssignments(string instanceId, RoleAssignmentQueryParameters queryParameters)
        {
            if (string.IsNullOrWhiteSpace(queryParameters?.Scope))
                return [];

            var resourcePath = ResourcePathUtils.ParseForRoleAssignmentScope(
                queryParameters.Scope,
                _settings.InstanceIds);

            return _roleAssignmentStores[instanceId].RoleAssignments
                .Where(ra => resourcePath.IncludesResourcePath(ra.ScopeResourcePath!))
                .ToList();
        }

        #endregion

        #region Manage secret keys

        /// <inheritdoc/>
        public List<SecretKey> GetSecretKeys(string instanceId, string contextId)
        {
            if (_secretKeyCaches.TryGetValue(instanceId, out var secretKeyCache))
            {
                var persistedSecretKeys = secretKeyCache.GetKeys(contextId);

                return persistedSecretKeys.Select(x => new SecretKey()
                {
                    Id = x.Id,
                    ContextId = contextId,
                    InstanceId = x.InstanceId,
                    Description = x.Description,
                    Active = x.Active,
                    ExpirationDate = x.ExpirationDate,
                }).ToList();
            }

            return [];
        }

        /// <inheritdoc/>
        public async Task<string?> UpsertSecretKey(string instanceId, SecretKey secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey.Id)
                || !Guid.TryParse(secretKey.Id, out var keyId)
                || string.IsNullOrWhiteSpace(secretKey.InstanceId)
                || string.IsNullOrWhiteSpace(secretKey.ContextId))
                throw new AuthorizationException("The secret key is invalid.");

            if (_secretKeyStores.TryGetValue(instanceId, out var secretKeyStore))
            {
                // Fill information into persisted key
                var persistedSecretKey = new PersistedSecretKey()
                {
                    Id = secretKey.Id,
                    InstanceId = instanceId,
                    ContextId = secretKey.ContextId,
                    Description = secretKey.Description,
                    ExpirationDate = secretKey.ExpirationDate,
                    Active = secretKey.Active
                };

                if (secretKeyStore.SecretKeys.TryGetValue(secretKey.ContextId, out var persistedSecretKeys))
                {
                    var existingKey = persistedSecretKeys.Where(x => x.Id == secretKey.Id).SingleOrDefault();

                    if (existingKey != null)
                    {
                        await PersistSecretKey(persistedSecretKey, false);
                        return null;
                    }
                }

                var rand = RandomNumberGenerator.Create();

                var secretBytes = new byte[128];
                rand.GetBytes(secretBytes);
                var secret = Base58.Encode(secretBytes);

                var saltBytes = new byte[16];
                rand.GetBytes(saltBytes);
                var salt = Base58.Encode(saltBytes);

                var argon2 = new Argon2id(secretBytes)
                {
                    DegreeOfParallelism = 16,
                    MemorySize = 8192,
                    Iterations = 40,
                    Salt = saltBytes
                };

                var hashBytes = argon2.GetBytes(128);
                var hash = Base58.Encode(hashBytes);

                // Save this API key salt and hash the key vault
                await _azureKeyVaultService.SetSecretValueAsync(persistedSecretKey.SaltKeyVaultSecretName, salt);
                await _azureKeyVaultService.SetSecretValueAsync(persistedSecretKey.HashKeyVaultSecretName, hash);

                await PersistSecretKey(persistedSecretKey, true);

                // Construct client key
                var clientSecretKey = new ClientSecretKey()
                {
                    InstanceId = persistedSecretKey.InstanceId,
                    ContextId = persistedSecretKey.ContextId,
                    Id = persistedSecretKey.Id,
                    ClientSecret = secret
                };

                // Return the string representation of the client key
                return clientSecretKey.ClientSecretString;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task DeleteSecretKey(string instanceId, string contextId, string secretKeyId)
        {
            var secretKeysStoreFile = $"/{instanceId.ToLower()}-secret-keys.json";

            try
            {
                await _syncRoot.WaitAsync();

                if (_secretKeyStores.TryGetValue(instanceId, out var secretKeyStore))
                {
                    if (secretKeyStore.SecretKeys.TryGetValue(contextId, out var persistedSecretKeys))
                    {
                        var existingKey = persistedSecretKeys.Where(x => x.Id == secretKeyId).SingleOrDefault();
                        if (existingKey != null)
                        {
                            persistedSecretKeys.Remove(existingKey);

                            await _azureKeyVaultService.RemoveSecretAsync(existingKey.SaltKeyVaultSecretName);
                            await _azureKeyVaultService.RemoveSecretAsync(existingKey.HashKeyVaultSecretName);

                            _secretKeyStores.AddOrUpdate(instanceId, secretKeyStore, (k, v) => secretKeyStore);
                            _secretKeyCaches[instanceId].RemovePersistedSecretKey(existingKey);

                            await _storageService.WriteFileAsync(
                                    SECRET_KEYS_CONTAINER_NAME,
                                    secretKeysStoreFile,
                                    JsonSerializer.Serialize(secretKeyStore),
                                    default,
                                    default);
                        }
                    }
                }
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<SecretKeyValidationResult> ValidateSecretKey(ClientSecretKey clientSecretKey)
        {
            // Fetch the matching persisted key
            var persistedSecretKey = await GetPersistedSecretKey(clientSecretKey.InstanceId, clientSecretKey.ContextId, clientSecretKey.Id);
            if (persistedSecretKey == null)
            {
                _logger.LogWarning($"The key with id {clientSecretKey.Id} was not found in instance {clientSecretKey.InstanceId} and context {clientSecretKey.ContextId}.");
                return new SecretKeyValidationResult() { Valid = false };
            }

            if (TestKeys(clientSecretKey, persistedSecretKey))
            {
                return new SecretKeyValidationResult() { Valid = true };
            }
            else
            {
                _logger.LogWarning("Invalid API key hash.");
                return new SecretKeyValidationResult() { Valid = false };
            }
        }

        #endregion

        private async Task PersistSecretKey(PersistedSecretKey persistedSecretKey, bool newKey)
        {
            var secretKeysStoreFile = $"/{persistedSecretKey.InstanceId.ToLower()}-secret-keys.json";

            try
            {
                await _syncRoot.WaitAsync();

                var fileContent = await _storageService.ReadFileAsync(SECRET_KEYS_CONTAINER_NAME, secretKeysStoreFile, default);
                var secretKeyStore = JsonSerializer.Deserialize<SecretKeyStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));
                if (secretKeyStore != null)
                {
                    var contextIdExists = secretKeyStore.SecretKeys.Any(x => x.Key == persistedSecretKey.ContextId);

                    if (!contextIdExists)
                        secretKeyStore.SecretKeys.Add(persistedSecretKey.ContextId, [persistedSecretKey]);
                    else
                    {
                        var exists = secretKeyStore.SecretKeys[persistedSecretKey.ContextId].Where(x => x.Id == persistedSecretKey.Id).SingleOrDefault();

                        if (exists != null)
                            secretKeyStore.SecretKeys[persistedSecretKey.ContextId].Remove(exists);

                        secretKeyStore.SecretKeys[persistedSecretKey.ContextId].Add(persistedSecretKey);
                    }

                    _secretKeyStores.AddOrUpdate(persistedSecretKey.InstanceId, secretKeyStore, (k, v) => secretKeyStore);
                    _secretKeyCaches[persistedSecretKey.InstanceId].AddOrUpdatePersistedSecretKey(persistedSecretKey);

                    await _storageService.WriteFileAsync(
                            SECRET_KEYS_CONTAINER_NAME,
                            secretKeysStoreFile,
                            JsonSerializer.Serialize(secretKeyStore),
                            default,
                            default);
                }
            }
            finally
            {
                _syncRoot.Release();
            }
        }

        private static bool TryDecode(string text, int expectedLength, out byte[] bytes)
        {
            bytes = Array.Empty<byte>();

            // You would think that a function called "TryDecode" would not throw an exception.
            // However, you would be wrong.
            try
            {
                bytes = new byte[expectedLength];
                var success = Base58.TryDecode(text, bytes, out var numBytesWritten);
                return success && numBytesWritten == expectedLength;
            }
            catch
            {
                return false;
            }
        }

        private static bool TestKeys(ClientSecretKey clientSecretKey, PersistedSecretKey persistedSecretKey)
        {
            if (!TryDecode(clientSecretKey.ClientSecret, 128, out var secretBytes))
                return false;

            if (!TryDecode(persistedSecretKey.Salt!, 16, out var saltBytes))
                return false;

            var argon2 = new Argon2id(secretBytes)
            {
                DegreeOfParallelism = 16,
                MemorySize = 8192,
                Iterations = 40,
                Salt = saltBytes
            };

            var hashBytes = argon2.GetBytes(128);
            var computedHash = Base58.Encode(hashBytes);

            return string.Equals(computedHash, persistedSecretKey.Hash);
        }

        private async Task<PersistedSecretKey?> GetPersistedSecretKey(string instanceId, string contextId, string secretKeyId)
        {
            if (_secretKeyStores.TryGetValue(instanceId, out var secretKeyStore))
            {
                if (secretKeyStore.SecretKeys.TryGetValue(contextId, out var persistedSecretKeys))
                {
                    var existingKey = persistedSecretKeys.Where(x => x.Id.ToLower() == secretKeyId.ToString().ToLower()).SingleOrDefault();

                    if (existingKey != null)
                    {
                        existingKey.Salt = await _azureKeyVaultService.GetSecretValueAsync(existingKey.SaltKeyVaultSecretName);
                        existingKey.Hash = await _azureKeyVaultService.GetSecretValueAsync(existingKey.HashKeyVaultSecretName);

                        return existingKey;
                    }
                }
            }

            return null;
        }
    }
}
