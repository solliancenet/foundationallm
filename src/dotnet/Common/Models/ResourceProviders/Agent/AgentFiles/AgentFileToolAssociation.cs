using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Agent file tool association.
    /// </summary>
    public class AgentFileToolAssociation :  ResourceBase
    {
        /// <summary>
        /// Agent file object id.
        /// </summary>
        [JsonPropertyName("file_object_id")]
        public required string FileObjectId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// Currently used when associating an agent file resource with a tool.
        /// </summary>
        [JsonPropertyName("associated_resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties>? AssociatedResourceObjectIds { get; set; }

        /// <summary>
        /// When a file is used with the OpenAI assistants API tool, associate the file with the OpenAI file ID.
        /// </summary>
        [JsonPropertyName("openai_file_id")]
        public string? OpenAIFileId { get; set; }
    }
}
