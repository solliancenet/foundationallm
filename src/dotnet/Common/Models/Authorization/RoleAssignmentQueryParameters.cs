using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Parameters for querying role assignments.
    /// </summary>
    public class RoleAssignmentQueryParameters
    {
        /// <summary>
        /// The role assignment scope (resource object id).
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope {  get; set; }
    }
}
