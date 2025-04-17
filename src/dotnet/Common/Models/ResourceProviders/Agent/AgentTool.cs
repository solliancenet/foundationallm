using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json;
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
        /// Gets or sets the category of the tool.
        /// </summary>
        [JsonPropertyName("category")]
        public string Category { get; set; } = AgentToolCategories.Generic;

        /// <summary>
        /// Gets or sets the package name of the tool.
        /// For internal tools, this value will be FoundationaLLM
        /// For external tools, this value will be the name of the package.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the underlying class name of the tool.
        /// </summary>
        [JsonPropertyName("class_name")]
        public string? ClassName
        {
            get => string.IsNullOrWhiteSpace(_className) ? Name : _className;
            set => _className = value ?? string.Empty;
        }
        private string _className = string.Empty;

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// </summary>
        [JsonPropertyName("resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties> ResourceObjectIds { get; set; } = [];

        /// <summary>
        /// Gets or sets a dictionary of properties that are specific to the tool.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object>? Properties { get; set; } = [];

        /// <summary>
        /// Tries to get the resource object identifiers with the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role being searched for. This is the value of the "object_role" property.</param>
        /// <param name="resourceObjectIds">The resulting list of resource object identifiers.</param>
        /// <returns></returns>
        public bool TryGetResourceObjectIdsWithRole(string roleName, out List<string>? resourceObjectIds)
        {
            resourceObjectIds = default;

            resourceObjectIds = [.. ResourceObjectIds.Values
                .Where(roid => roid.HasObjectRole(roleName))
                .Select(roid => roid.ObjectId)];

            return true;
        }

        /// <summary>
        /// Tries to get the value of a property.
        /// </summary>
        /// <typeparam name="T">The type of the property being retrieved.</typeparam>
        /// <param name="propertyName">The name of the property being retrieved.</param>
        /// <param name="propertyValue">The resultig property value.</param>
        /// <returns><see langword="true"/> if the property value was successfull retrieved, <see langword="false"/> otherwise.</returns>
        public bool TryGetPropertyValue<T>(string propertyName, out T? propertyValue)
        {
            propertyValue = default;

            if (Properties == null)
                return false;

            if (Properties.TryGetValue(propertyName, out var value))
            {
                propertyValue = ((JsonElement)value).Deserialize<T>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the knowledge search settings if they exists.
        /// </summary>
        /// <returns>An object containing the knowledge search settings.</returns>
        public KnowledgeSearchSettings? GetKnowledgeSearchSettings() =>
            TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.FileUploadDataPipeline, out var dataPipelineResourceObjectIds)
            && dataPipelineResourceObjectIds != null
            && dataPipelineResourceObjectIds.Count == 1
            && TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.VectorDatabase, out var vectorDatabaseObjectIds)
            && vectorDatabaseObjectIds != null
            && vectorDatabaseObjectIds.Count == 1
            ? new KnowledgeSearchSettings
            {
                FileUploadDataPipelineObjectId = ResourceObjectIds[dataPipelineResourceObjectIds[0]].ObjectId,
                FileUploadVectorDatabaseObjectId = ResourceObjectIds[vectorDatabaseObjectIds[0]].ObjectId
            }
            : null;
    }
}
