using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Agent.Models.Resources
{
    /// <summary>
    /// Provides details about an agent.
    /// </summary>
    public class AgentReference : ResourceReference
    {
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

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                AgentTypes.Basic => typeof(AgentBase),
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                AgentTypes.AgentFile => typeof(AgentFile),
                AgentTypes.Workflow => typeof(Workflow),
                AgentTypes.Tool => typeof(Tool),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };
    }
}
