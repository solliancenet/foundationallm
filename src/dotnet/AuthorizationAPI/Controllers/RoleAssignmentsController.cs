﻿using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Authorization.API.Controllers
{
    /// <summary>
    /// Provides security endpoints.
    /// </summary>
    /// <param name="authorizationCore">The <see cref="IAuthorizationCore"/> service used to process authorization requests.</param>
    [Authorize(Policy = AuthorizationPolicyNames.MicrosoftEntraIDNoScopes)]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/roleassignments")]
    public class RoleAssignmentsController(
        IAuthorizationCore authorizationCore) : Controller
    {
        private readonly IAuthorizationCore _authorizationCore = authorizationCore;

        #region IAuthorizationCore

        /// <summary>
        /// Returns a list of role assignments for the specified instance.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>The list of all role assignments for the specified instance.</returns>
        [HttpPost("query")]
        public IActionResult GetRoleAssignments(string instanceId, [FromBody] RoleAssignmentQueryParameters queryParameters) =>
            new OkObjectResult(_authorizationCore.GetRoleAssignments(instanceId, queryParameters));

        /// <summary>
        /// Assigns a role to an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignmentRequest">The role assignment request.</param>
        /// <returns>The role assignment result.</returns>
        [HttpPost]
        public async Task<IActionResult> AssignRole(string instanceId, RoleAssignmentRequest roleAssignmentRequest) =>
            new OkObjectResult(await _authorizationCore.CreateRoleAssignment(instanceId, roleAssignmentRequest));

        /// <summary>
        /// Revokes a role from an Entra ID user or group.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="roleAssignment">The role assignment object identifier.</param>
        /// <returns>The role assignment result.</returns>
        [HttpDelete("{*roleAssignment}")]
        public async Task<IActionResult> RevokeRoleAssignment(string instanceId, string roleAssignment) =>
            new OkObjectResult(await _authorizationCore.DeleteRoleAssignment(instanceId, roleAssignment));

        #endregion
    }
}
