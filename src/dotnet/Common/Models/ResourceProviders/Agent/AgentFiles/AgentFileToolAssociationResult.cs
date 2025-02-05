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
    }
}
