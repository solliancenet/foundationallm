using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;

namespace FoundationaLLM.AuthorizationEngine.Interfaces
{
    /// <summary>
    /// Defines the methods for authorization core.
    /// </summary>
    public interface IAuthorizationCore
    {
        /// <summary>
        /// Checks if a specified security principal is allowed to process authorization requests. 
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="securityPrincipalId">The id of the security principal whose authorization is checked.</param>
        /// <returns>True if the security principal is allowed to process authorization requests.</returns>
        bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId);

        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> containing the details of the authorization request.</param>
        /// <returns>An <see cref="ActionAuthorizationResult"/> indicating whether the requested authorization was successfull or not for each resource path.</returns>
        ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest);

        /// <summary>
        /// Creates a role assignment for a specified security principal.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The role assignment request.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentOperationResult> CreateRoleAssignment(string instanceId, RoleAssignmentRequest roleAssignmentRequest);

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentOperationResult> DeleteRoleAssignment(string instanceId, string roleAssignment);

        
        /// <summary>
        /// Returns a list of role assignments for the specified instance and resource path.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="queryParameters">The <see cref="RoleAssignmentQueryParameters"/> providing the inputs for filtering the role assignments.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        List<RoleAssignment> GetRoleAssignments(string instanceId, RoleAssignmentQueryParameters queryParameters);

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
        List<SecretKey> GetSecretKeys(string instanceId, string contextId);

        /// <summary>
        /// Creates a new or updates an existing <see cref="SecretKey"/> item.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="secretKey">The <see cref="SecretKey"/> item containing the properties of the secret key being created or updated.</param>
        /// <returns>If the secret key is being created, it returns the secret value of the key. Otherwise, returns <c>null</c>.</returns>
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
