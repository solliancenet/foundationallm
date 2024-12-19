using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides a workflow configuration for an agent.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureOpenAIAssistantsAgentWorkflow), AgentWorkflowTypes.AzureOpenAIAssistants)]
    [JsonDerivedType(typeof(LangChainExpressionLanguageAgentWorkflow), AgentWorkflowTypes.LangChainExpressionLanguage)]
    [JsonDerivedType(typeof(LangGraphReactAgentWorkflow), AgentWorkflowTypes.LangGraphReactAgent)]
    [JsonDerivedType(typeof(ExternalAgentWorkflow), AgentWorkflowTypes.ExternalAgentWorkflow)]
    public class AgentWorkflowBase
    {        
        /// <summary>
        /// The workflow resource associated with the agent.
        /// </summary>
        [JsonPropertyName("type")]
        public virtual string? Type { get; set; }

        /// <summary>
        /// The name of the workflow.
        /// </summary>        
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the package name of the workflow.
        /// For internal workflows, this value will be FoundationaLLM
        /// For external workflows, this value will be the name of the package.
        /// </summary>
        [JsonPropertyName("package_name")]
        public required string PackageName { get; set; }

        /// <summary>
        /// The host of the workflow environment.
        /// </summary>
        [JsonPropertyName("workflow_host")]
        public string? WorkflowHost { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of resource objects.
        /// </summary>
        [JsonPropertyName("resource_object_ids")]
        public Dictionary<string, ResourceObjectIdProperties> ResourceObjectIds { get; set; } = [];

        /// <summary>
        /// Gets the main AI model object identifier.
        /// </summary>
        [JsonIgnore]
        public string? MainAIModelObjectId =>
            ResourceObjectIds.Values
                .FirstOrDefault(
                    roid => roid.HasObjectRole(ResourceObjectIdPropertyValues.MainModel))
                ?.ObjectId;
    }
}
