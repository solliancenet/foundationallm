using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using FoundationaLLM.Common.Utils;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a PBAC policy assignment.
    /// </summary>
    public class PolicyAssignment : AssignmentBase
    {
        /// <summary>
        /// The unique identifier of the policy definition.
        /// </summary>
        [JsonPropertyName("policy_definition_id")]
        public required string PolicyDefinitionId { get; set; }

        /// <summary>
        /// The <see cref="PolicyDefinition"/> referenced by the <see cref="PolicyDefinitionId"/> property.
        /// </summary>
        [JsonIgnore]
        public PolicyDefinition? PolicyDefinition { get; set; }

        /// <summary>
        /// Enriches the policy assignment with additional information.
        /// </summary>
        /// <param name="allowedInstanceIds">The list of FoundationaLLM instance identifiers used as context for the enrichment.</param>
        /// <remarks>
        /// This method is called when the policy assignments are loaded into memory.
        /// Besides the actual enrichment, it also ensures that the <see cref="AssignmentBase.Scope"/> property is set correctly.
        /// </remarks>
        public void Enrich(List<string> allowedInstanceIds)
        {
            ScopeResourcePath = ResourcePathUtils.ParseForPolicyAssignmentScope(
                Scope,
                allowedInstanceIds);

            PolicyDefinition = PolicyDefinitions.All[PolicyDefinitionId];
        }
    }
}
