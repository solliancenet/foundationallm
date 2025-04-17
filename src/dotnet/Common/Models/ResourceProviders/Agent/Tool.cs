using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Tool resource.
    /// </summary>
    public class Tool : ResourceBase
    {
        /// <summary>
        /// Gets or sets the category of the tool.
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; } = AgentToolCategories.Generic;
    }
}
