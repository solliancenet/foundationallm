using FoundationaLLM.Common.Models.Orchestration.Request;

namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants for the keys that can be added to the <see cref="LLMCompletionRequest.Objects"/> dictionary.
    /// </summary>
    public static class CompletionRequestObjectsKeys
    {
        /// <summary>
        /// The key name for the dictionary containing names and descriptions of agents other than the completion request's agent.
        /// This value should be a dictionary where keys are agent names and values are agent descriptions.
        /// </summary>
        public const string AllAgents = "AllAgents";

        /// <summary>
        /// The key name for the OpenAI assistant identifier value.
        /// </summary>
        public const string OpenAIAssistantsAssistantId = "OpenAI.Assistants.Assistant.Id";

        /// <summary>
        /// The key name for the OpenAI assistant thread identifier value.
        /// </summary>
        public const string OpenAIAssistantsThreadId = "OpenAI.Assistants.Thread.Id";

        /// <summary>
        /// The key name for the Azure AI Agent Service agent identifier value.
        /// </summary>
        public const string AzureAIAgentServiceAgentId = "AzureAI.AgentService.Agent.Id";

        /// <summary>
        /// The key name for the Azure AI Agent Service thread identifier value.
        /// </summary>
        public const string AzureAIAgentServiceThreadId = "AzureAI.AgentService.Thread.Id";

        /// <summary>
        /// The key name for the Gateway API EndpointConfiguration identifier value.
        /// </summary>
        public const string GatewayAPIEndpointConfiguration = "GatewayAPIEndpointConfiguration";

        /// <summary>
        /// The key name for the Context API EndpointConfiguration identifier value.
        /// </summary>
        public const string ContextAPIEndpointConfiguration = "ContextAPIEndpointConfiguration";

        /// <summary>
        /// The key name for the list of tool names that are registered with the agent.
        /// </summary>
        public const string ToolNames = "ToolNames";

        /// <summary>
        /// The key name for the FoundationaLLM instance identifier value.
        /// </summary>
        public const string InstanceId = "FoundationaLLM.InstanceId";

        /// <summary>
        /// All completion request objects dictionary keys.
        /// </summary>
        public readonly static string[] All = [
            AllAgents
        ];
    }
}
