using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Request object to associate a tool with an agent file.
    /// Agent and File are obtained from the request URL.
    /// </summary>
    public class AgentFileToolAssociationRequest
    {
        /// <summary>
        /// The agent file tool association matrix.
        /// </summary>
        [JsonPropertyName("agent_file_tool_associations")]
        public required Dictionary<string, Dictionary<string, bool>> AgentFileToolAssociations {  get; set; }
    }
}
