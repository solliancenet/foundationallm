using FoundationaLLM.Authorization.Models;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Interfaces
{
    /// <summary>
    /// Defines the methods for authorization core.
    /// </summary>
    public interface IAuthorizationCore
    {
        /// <summary>
        /// Processes an authorization request.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="authorizationRequest">The <see cref="ActionAuthorizationRequest"/> containing the details of the authorization request.</param>
        /// <returns>An <see cref="ActionAuthorizationResult"/> indicating whether the requested authorization was successfull or not for each resource path.</returns>
        ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest);

        /// <summary>
        /// Checks if a specified security principal is allowed to process authorization requests. 
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="securityPrincipalId">The id of the security principal whose authorization is checked.</param>
        /// <returns>True if the security principal is allowed to process authorization requests.</returns>
        bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId);

        /// <summary>
        /// Creates a role assignment for a specified security principal.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The role assignment request.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentResult> CreateRoleAssignment(string instanceId, RoleAssignmentRequest roleAssignmentRequest);

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        Task<RoleAssignmentResult> RevokeRoleAssignment(string instanceId, string roleAssignment);

        /// <summary>
        /// Returns a list of role names and a list of allowed actions for the specified scope.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="request">The get roles with actions request.</param>
        /// <returns>The get roles and actions result.</returns>
        Dictionary<string, RoleAssignmentsWithActionsResult> ProcessRoleAssignmentsWithActionsRequest(string instanceId, RoleAssignmentsWithActionsRequest request);

        /// <summary>
        /// Returns a list of role assignments for the specified instance and resource path.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="queryParameters">The <see cref="RoleAssignmentQueryParameters"/> providing the inputs for filtering the role assignments.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        List<RoleAssignment> GetRoleAssignments(string instanceId, RoleAssignmentQueryParameters queryParameters);
    }
}
