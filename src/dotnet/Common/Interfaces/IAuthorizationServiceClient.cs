using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines methods exposed by the Authorization service.
    /// </summary>
    public interface IAuthorizationServiceClient
    {
        /// <summary>
        /// Processes an action authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="action">The action identifier.</param>
        /// <param name="resourcePaths">The resource paths.</param>
        /// <param name="expandResourceTypePaths">A value indicating whether to expand resource type paths that are not authorized.</param>
        /// <param name="includeRoles">A value indicating whether to include roles in the response.</param>
        /// <param name="includeActions">A value indicating whether to include authorizable actions in the response.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <remarks>
        /// <para>
        /// If the action specified by <paramref name="action"/> is not authorized for a resource type path,
        /// and <paramref name="expandResourceTypePaths"/> is set to <c>true</c>, the response will include
        /// any authorized resource paths matching the resource type path.
        /// </para>
        /// <para>
        /// If <paramref name="includeRoles"/> is set to <c>true</c>, for each authrorized resource path,
        /// the response will include the roles assigned directly or indirectly to the resource path.
        /// </para>
        /// <para>
        /// If <paramref name="action"/> is set to <c>true</c>, for each authorized resource path,
        /// the response will include the autorizable actions assigned directly or indirectly to the resource path.</para>
        /// </remarks>
        /// <returns>An <see cref="ActionAuthorizationResult"/> containing the result of the processing.</returns>
        Task<ActionAuthorizationResult> ProcessAuthorizationRequest(
            string instanceId,
            string action,
            List<string> resourcePaths,
            bool expandResourceTypePaths,
            bool includeRoles,
            bool includeActions,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a new role assignment.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The <see cref="RoleAssignmentRequest"/> containing the details of the role assignment to be created.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>A <see cref="RoleAssignmentOperationResult"/> containing information about the result of the operation.</returns>
        Task<RoleAssignmentOperationResult> CreateRoleAssignment(
            string instanceId,
            RoleAssignmentRequest roleAssignmentRequest,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Returns a list of role assignments.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="queryParameters">The <see cref="RoleAssignmentQueryParameters"/> providing the inputs for filtering the role assignments.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        Task<List<RoleAssignment>> GetRoleAssignments(
            string instanceId,
            RoleAssignmentQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Deletes a role assignment.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <param name="userIdentity">The user identity.</param>
        /// <returns>A <see cref="RoleAssignmentOperationResult"/> containing information about the result of the operation.</returns>
        Task<RoleAssignmentOperationResult> DeleteRoleAssignment(
            string instanceId,
            string roleAssignment,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Gets a list of <see cref="SecretKey"/> items that are associated with the specified instance and context.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="contextId">The identifier of the context for which the secret keys are retrieved.</param>
        /// <returns>A list of <see cref="SecretKey"/> items.</returns>
        /// <remarks>
        /// Each consumer of secret keys should have a unique context identifier.
        /// For example, FoundationaLLM resource providers could use the resource object identifier as the context identifier.
        /// </remarks>
        Task<List<SecretKey>> GetSecretKeys(string instanceId, string contextId);

        /// <summary>
        /// Creates a new or updates an existing <see cref="SecretKey"/> item.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="secretKey">The <see cref="SecretKey"/> item containing the properties of the secret key being created or updated.</param>
        /// <returns>If the secret bey is being created, it returns the secret value of the key. Otherwise, returns <c>null</c>.</returns>
        Task<string?> UpsertSecretKey(string instanceId, SecretKey secretKey);

        /// <summary>
        /// Deletes a specified secret key.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="contextId">The identifier of the context containing the secret key to delete.</param>
        /// <param name="secretKeyId">The unique identifier of the secret key being deleted.</param>
        Task DeleteSecretKey(string instanceId, string contextId, string secretKeyId);

        /// <summary>
        /// Validates a secret key value.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="contextId">The identifier of the context containing the secret key to validate.</param>
        /// <param name="secretKeyValue">The secret value of the key.</param>
        /// <returns>A <see cref="SecretKeyValidationResult"/> item with the results of the validation.</returns>
        /// <remarks>
        /// Each valid secret key has an associated virtual identity that is returned in the <see cref="SecretKeyValidationResult.VirtualIdentity"/> property.
        /// It is the responsibility of the caller to use the virtual identity to authorize the request.
        /// </remarks>
        Task<SecretKeyValidationResult> ValidateSecretKey(string instanceId, string contextId, string secretKeyValue);
    }
}
