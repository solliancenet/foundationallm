using FoundationaLLM.AuthorizationEngine.Models;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.AuthorizationEngine.Services
{
    /// <summary>
    /// In-memory cache for policy assignments.
    /// </summary>
    public class PolicyAssignmentCache
    {
        private readonly Dictionary<string, List<PolicyAssignment>> _scopePolicyAssignments;

        /// <summary>
        /// Creates a new instance of the <see cref="PolicyAssignmentCache"/> class.
        /// </summary>
        /// <param name="policyAssignmentStore">The policy assignment store containing the policy assignments used to initialize the cache.</param>
        public PolicyAssignmentCache(
            PolicyAssignmentStore policyAssignmentStore) =>
                _scopePolicyAssignments = policyAssignmentStore.PolicyAssignments
                    .GroupBy(pa => pa.Scope)
                    .ToDictionary(g => g.Key, g => g.ToList());

        public List<PolicyAssignment> GetPolicyAssignments(string scope) =>
            _scopePolicyAssignments.TryGetValue(scope, out var policyAssignments)
                ? policyAssignments
                : [];
    }
}
