using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.AuthorizationEngine.Models
{
    /// <summary>
    /// Models the content of the role assignments store managed by the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public class RoleAssignmentStore
    {
        /// <summary>
        /// The unique identifier of the FoundationaLLM instance.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public required string InstanceId { get; set; }

        /// <summary>
        /// The list of all role assignments in the FoundationaLLM instance.
        /// </summary>
        [JsonPropertyName("role_assignments")]
        public required List<RoleAssignment> RoleAssignments { get; set; } = [];

        /// <summary>
        /// Loads calculated properties for all role assignments.
        /// </summary>
        public void EnrichRoleAssignments()
        {
            var allowedInstanceIds = new List<string>() { InstanceId };

            foreach (var roleAssignment in RoleAssignments)
                roleAssignment.Enrich(allowedInstanceIds);
        }
    }
}
