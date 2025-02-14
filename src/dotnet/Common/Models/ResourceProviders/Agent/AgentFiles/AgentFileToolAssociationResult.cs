using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Represents the result of associating a tool with a file.
    /// </summary>
    public class AgentFileToolAssociationResult
    {
        /// <summary>
        /// Indicates if the tool association was successful.
        /// </summary>
        [JsonPropertyName("success")]
        public required bool Success { get; set; }

        /// <summary>
        /// Final agent file tool associations.
        /// </summary>
        [JsonPropertyName("agent_file_tool_associations")]
        public required List<AgentFileToolAssociation> AgentFileToolAssociations { get; set; }

        /// <summary>
        /// Agent file tool association errors.
        /// </summary>
        [JsonPropertyName("errors")]
        public List<AgentFileToolAssociationError>? Errors { get; set; }
    }
}
