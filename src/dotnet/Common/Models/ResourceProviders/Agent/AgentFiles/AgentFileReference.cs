using FoundationaLLM.Common.Exceptions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles
{
    /// <summary>
    /// Contains a reference to an file.
    /// </summary>
    public class AgentFileReference : FileReference
    {
        /// <summary>
        /// The instance unique identifier.
        /// </summary>
        public required string InstanceId { get; set; }

        /// <summary>
        /// The agent name.
        /// </summary>
        public required string AgentName { get; set; }

        /// <summary>
        /// The object type of the resource.
        /// </summary>
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                AgentTypes.AgentFile => typeof(AgentFile),
                _ => throw new ResourceProviderException($"The resource type {Type} is not supported.")
            };
    }
}

