using FoundationaLLM.Common.Models.Authorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Represents an authorization assignment.
    /// </summary>
    /// <remarks>
    /// This class should never be used directly.
    /// Instead, use <see cref="RoleAssignment"/> or <see cref="PolicyAssignment"/>.
    /// </remarks>
    public class AssignmentBase : ResourceBase
    {
        /// <summary>
        /// The unique identifier of the security principal which is the target of the assignment.
        /// </summary>
        [JsonPropertyName("principal_id")]
        public required string PrincipalId { get; set; }

        /// <summary>
        /// The type of the security principal which is the target of the assignment. Can be User, Group, or ServicePrincipal.
        /// </summary>
        [JsonPropertyName("principal_type")]
        public required string PrincipalType { get; set; }

        /// <summary>
        /// The scope at which the assignment is made.
        /// </summary>
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }

        /// <summary>
        /// The <see cref="ResourcePath"/> resulting from parsing the scope path.
        /// </summary>
        [JsonIgnore]
        public ResourcePath? ScopeResourcePath { get; set; }
    }
}
