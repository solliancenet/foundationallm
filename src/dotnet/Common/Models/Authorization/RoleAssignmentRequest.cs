using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a role assignment request.
    /// </summary>
    public class RoleAssignmentRequest
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(-5)]
        public required string Name { get; set; }

        /// <summary>
        /// The description of the resource.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonPropertyOrder(-2)]
        public string? Description { get; set; }

        /// <summary>
        /// The unique identifier of the role definition.
        /// </summary>
        [JsonPropertyName("role_definition_id")]
        public required string RoleDefinitionId { get; set; }

        /// <summary>
        /// The unique identifier of the security principal to which the role is assigned.
        /// </summary>
        [JsonPropertyName("principal_id")]
        public required string PrincipalId { get; set; }

        /// <summary>
        /// The type of the security principal to which the role is assigned. Can be User, Group, or ServicePrincipal.
        /// </summary>
        [JsonPropertyName("principal_type")]
        public required string PrincipalType { get; set; }

        /// <summary>
        /// The scope at which the role is assigned.
        /// </summary>
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }

        /// <summary>
        /// The entity who created the role assignment request.
        /// </summary>
        [JsonPropertyName("created_by")]
        [JsonPropertyOrder(502)]
        public string? CreatedBy { get; set; }
    }
}
