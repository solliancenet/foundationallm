using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Prvides the settings for a tool that is registered with the agent.
    /// </summary>
    public class AgentTool
    {
        /// <summary>
        /// Gets or sets the name of the tool.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the package name of the tool.
        /// For internal tools, this value will be FoundationaLLM
        /// For external tools, this value will be the name of the package.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// </summary>
        [JsonPropertyName("resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties> ResourceObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of properties that are specific to the tool.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];
    }
}
