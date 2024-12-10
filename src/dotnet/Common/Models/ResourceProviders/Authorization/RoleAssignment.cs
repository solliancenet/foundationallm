using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Utils;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Represents an RBAC role assignment.
    /// </summary>
    public class RoleAssignment : AssignmentBase
    {
        /// <summary>
        /// The unique identifier of the role definition.
        /// </summary>
        [JsonPropertyName("role_definition_id")]
        public required string RoleDefinitionId { get; set; }

        /// <summary>
        /// The <see cref="RoleDefinition"/> referenced by the <see cref="RoleDefinitionId"/> property.
        /// </summary>
        [JsonIgnore]
        public RoleDefinition? RoleDefinition { get; set; }

        /// <summary>
        /// The explicit list of all allowed actions resulting from expanding all wildcards.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> AllowedActions { get; set; } = [];

        /// <summary>
        /// Enriches the role assignment with additional information.
        /// </summary>
        /// <param name="allowedInstanceIds">The list of FoundationaLLM instance identifiers used as context for the enrichment.</param>
        /// <remarks>
        /// This method is called when the role assignments are loaded into memory.
        /// Besides the actual enrichment, it also ensures that the <see cref="AssignmentBase.Scope"/> property is set correctly.
        /// </remarks>
        public void Enrich(List<string> allowedInstanceIds)
        {
            ScopeResourcePath = ResourcePathUtils.ParseForRoleAssignmentScope(
                Scope,
                allowedInstanceIds);

            RoleDefinition = RoleDefinitions.All[RoleDefinitionId];
            AllowedActions = new HashSet<string>(
                RoleDefinition.GetAllowedActions());
        }
    }
}
