using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Base agent metadata model.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(KnowledgeManagementAgent), "knowledge-management")]
    public class AgentBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// Indicates whether sessions are enabled for the agent.
        /// </summary>
        [JsonPropertyName("sessions_enabled")]
        public bool SessionsEnabled { get; set; }
        /// <summary>
        /// The agent's conversation history configuration.
        /// </summary>
        [JsonPropertyName("conversation_history_settings")]
        public ConversationHistorySettings? ConversationHistorySettings { get; set; }
        /// <summary>
        /// The agent's Gatekeeper configuration.
        /// </summary>
        [JsonPropertyName("gatekeeper_settings")]
        public GatekeeperSettings? GatekeeperSettings { get; set; }


        /// <summary>
        /// Settings for the orchestration service.
        /// </summary>
        [JsonPropertyName("orchestration_settings")]
        public AgentOrchestrationSettings? OrchestrationSettings { get; set; }

        /// <summary>
        /// The object identifier of the <see cref="PromptBase"/> object providing the prompt for the agent.
        /// </summary>
        [JsonPropertyName("prompt_object_id")]
        public string? PromptObjectId { get; set; }
		
        /// <summary>
        /// The object identifier of the <see cref="AIModelBase"/> object providing the AI model for the agent.
        /// </summary>
        [JsonPropertyName("ai_model_object_id")]
        public string? AIModelObjectId { get; set; }

        /// <summary>
        /// List of capabilities that the agent supports.
        /// </summary>
        [JsonPropertyName("capabilities")]
        public string[]? Capabilities { get; set; }

        /// <summary>
        /// The agent's workflow configuration.
        /// </summary>
        [JsonPropertyName("workflow")]
        public AgentWorkflowBase? Workflow { get; set; }

        /// <summary>
        /// Gets or sets a list of tools that are registered with the agent.
        /// </summary>
        /// <remarks>
        /// The key is the name of the tool, and the value is the <see cref="AgentTool"/> object.
        /// </remarks>
        [JsonPropertyName("tools")]
        public AgentTool[] Tools { get; set; } = [];

        /// <summary>
        /// Gets or sets the object identifier of the virtual security group associated with the agent.
        /// </summary>\
        /// <remarks>
        /// The virtual security group is used to provide access to the agent for the virtual security principals.
        /// An example of such a virtual security principal is the virtual identity associated with an
        /// agent access token.
        /// </remarks>
        [JsonPropertyName("virtual_security_group_id")]
        public string? VirtualSecurityGroupId { get; set; }

        /// <summary>
        /// The object type of the agent.
        /// </summary>
        [JsonIgnore]
        public Type AgentType =>
            Type switch
            {
                AgentTypes.KnowledgeManagement => typeof(KnowledgeManagementAgent),
                _ => throw new ResourceProviderException($"The agent type {Type} is not supported.")
            };

        /// <summary>
        /// Checks whether the agent has a specified capbability.
        /// </summary>
        /// <param name="capabilityName">The name of the capability.</param>
        /// <returns>True if the agent has the capability, False otherwise.</returns>
        public bool HasCapability(string capabilityName) =>
            Capabilities?.Contains(capabilityName) ?? false;
    }

    /// <summary>
    /// Agent conversation history settings.
    /// </summary>
    public class ConversationHistorySettings
    {
        /// <summary>
        /// Indicates whether the conversation history is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// The maximum number of turns to store in the conversation history.
        /// </summary>
        [JsonPropertyName("max_history")]
        public int MaxHistory { get; set; }
    }

    /// <summary>
    /// Agent Gatekeeper settings.
    /// </summary>
    public class GatekeeperSettings
    {
        /// <summary>
        /// Indicates whether to abide by or override the system settings for the Gatekeeper.
        /// </summary>
        [JsonPropertyName("use_system_setting")]
        public bool UseSystemSetting { get; set; }

        /// <summary>
        /// If <see cref="UseSystemSetting"/> is false, provides Gatekeeper feature selection.
        /// </summary>
        [JsonPropertyName("options")]
        public string[]? Options { get; set; }
    }

}
