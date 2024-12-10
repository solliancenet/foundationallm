using FoundationaLLM.AuthorizationEngine.Models;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;

namespace FoundationaLLM.AuthorizationEngine.Services
{
    /// <summary>
    /// In-memory cache for role assignments.
    /// </summary>
    public class RoleAssignmentCache
    {
        private readonly Dictionary<string, List<RoleAssignment>> _principalRoleAssignments;
        private readonly Dictionary<string, RoleAssignment> _roleAssignments;

        /// <summary>
        /// Creates a new instance of the <see cref="RoleAssignmentCache"/> class.
        /// </summary>
        /// <param name="roleAssignmentStore">The role assignment store containing the role assignments used to initialize the cache.</param>
        public RoleAssignmentCache(
            RoleAssignmentStore roleAssignmentStore)
        {
            _principalRoleAssignments = roleAssignmentStore.RoleAssignments
                .GroupBy(ra => ra.PrincipalId)
                .ToDictionary(g => g.Key, g => g.ToList());

            _roleAssignments = roleAssignmentStore.RoleAssignments
                .ToDictionary(ra => ra.ObjectId!);
        }

        /// <summary>
        /// Adds or updates a role assignment in the cache.
        /// </summary>
        /// <param name="roleAssignment">The <see cref="RoleAssignment"/> to add or update in the cache.</param>
        public void AddOrUpdateRoleAssignment(RoleAssignment roleAssignment)
        {
            if (!_principalRoleAssignments.TryGetValue(roleAssignment.PrincipalId, out var roleAssignmentsForPrincipal))
            {
                roleAssignmentsForPrincipal = [];
                _principalRoleAssignments[roleAssignment.PrincipalId] = roleAssignmentsForPrincipal;
            }

            var existingRoleAssignment = roleAssignmentsForPrincipal.FirstOrDefault(ra => ra.ObjectId == roleAssignment.ObjectId);
            if (existingRoleAssignment != null)
                roleAssignmentsForPrincipal.Remove(existingRoleAssignment);
            roleAssignmentsForPrincipal.Add(roleAssignment);

            _roleAssignments[roleAssignment.ObjectId!] = roleAssignment;
        }

        /// <summary>
        /// Removes a role assignment from the cache.
        /// </summary>
        /// <param name="roleAssignmentId">The unique identifier of the role assigment to be removed.</param>
        public void RemoveRoleAssignment(string roleAssignmentId)
        {
            if (_roleAssignments.TryGetValue(roleAssignmentId, out var roleAssignment))
            {
                _roleAssignments.Remove(roleAssignmentId);
                if (_principalRoleAssignments.TryGetValue(roleAssignment.PrincipalId, out var roleAssignmentsForPrincipal))
                {
                    roleAssignmentsForPrincipal.Remove(roleAssignment);
                }
            }
        }

        /// <summary>
        /// Gets all role assignments for a principal.
        /// </summary>
        /// <param name="principalId">The unique identifier of the principal for which role assignments are retrieved.</param>
        /// <returns></returns>
        public List<RoleAssignment> GetRoleAssignments(string principalId) =>
            _principalRoleAssignments.TryGetValue(principalId, out var roleAssignments)
                ? roleAssignments
                : [];
    }
}
