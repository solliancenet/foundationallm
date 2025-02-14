using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Agent file tool error.
    /// </summary>
    public class AgentFileToolAssociationError
    {
        /// <summary>
        /// File object id.
        /// </summary>
        [JsonPropertyName("file_object_id")]
        public required string FileObjectId {  get; set; }

        /// <summary>
        /// Tool object id.
        /// </summary>
        [JsonPropertyName("tool_object_id")]
        public required string ToolObjectId { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        [JsonPropertyName("error_message")]
        public required string ErrorMessage { get; set; }
    }
}
