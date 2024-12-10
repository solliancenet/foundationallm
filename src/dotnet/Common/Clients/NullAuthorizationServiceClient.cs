using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Implements an authorization service that bypasses the Authorization API and allows all access by default.
    /// </summary>
    public class NullAuthorizationServiceClient : IAuthorizationServiceClient
    {
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
                    Authorized = true
                });

            await Task.CompletedTask;
            return new ActionAuthorizationResult { AuthorizationResults = defaultResults };
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentOperationResult { Success = true };
        }

        /// <inheritdoc/>
        public async Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return [];
        }

        /// <inheritdoc/>
        public async Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            return new RoleAssignmentOperationResult { Success = true };
        }

        /// <inheritdoc/>
        public async Task<List<SecretKey>> GetSecretKeys(string instanceId, string contextId)
        {
            await Task.CompletedTask;
            return [];
        }

        /// <inheritdoc/>
        public async Task<string?> UpsertSecretKey(string instanceId, SecretKey secretKey)
        {
            await Task.CompletedTask;
            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task DeleteSecretKey(string instanceId, string contextId, string secretKeyId) => await Task.CompletedTask;

        /// <inheritdoc/>
        public async Task<SecretKeyValidationResult> ValidateSecretKey(string instanceId, string contextId, string secretKeyValue)
        {
            await Task.CompletedTask;
            return new SecretKeyValidationResult() { Valid = true };
        }

    }
}
