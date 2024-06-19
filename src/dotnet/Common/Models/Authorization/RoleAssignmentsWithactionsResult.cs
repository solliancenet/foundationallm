using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of a GET roles with actions request.
    /// </summary>
    public class RoleAssignmentsWithActionsResult
    {
        /// <summary>
        /// List of authorized actions on the resource.
        /// </summary>
        [JsonPropertyName("actions")]
        public required List<string> Actions { get; set; }

        /// <summary>
        /// List of roles on the resource.
        /// </summary>
        [JsonPropertyName("roles")]
        public required List<string> Roles { get; set; }
    }
}
