using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of a role assignment operation.
    /// </summary>
    public class RoleAssignmentOperationResult
    {
        /// <summary>
        /// Indicates whether the role assignment operation was successful or not.
        /// </summary>
        [JsonPropertyName("success")]
        public required bool Success { get; set; }

        /// <summary>
        /// The reason for the result of the role assignment operation.
        /// </summary>
        [JsonPropertyName("result_reason")]
        public string? ResultReason { get; set; }
    }
}
