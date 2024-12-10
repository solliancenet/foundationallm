using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Defines a security role used in FoundationaLLM RBAC
    /// </summary>
    public class RoleDefinition : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override bool Deleted { get; set; }

        /// <summary>
        /// The list of scopes at which the role can be assigned.
        /// </summary>
        [JsonPropertyName("assignable_scopes")]
        [JsonPropertyOrder(1)]
        public List<string> AssignableScopes { get; set; } = [];

        /// <summary>
        /// The permissions associated with the security role definition.
        /// </summary>
        [JsonPropertyName("permissions")]
        [JsonPropertyOrder(2)]
        public List<RoleDefinitionPermissions> Permissions { get; set; } = [];

        public List<string> GetAllowedActions() =>
            Permissions.SelectMany(p => p.GetAllowedActions()).Distinct().ToList();
    }
}
