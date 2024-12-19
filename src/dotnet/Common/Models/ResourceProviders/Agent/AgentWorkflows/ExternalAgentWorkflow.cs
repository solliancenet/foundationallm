using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for an External Agent workflow loaded via an external module.
    /// </summary>
    public class ExternalAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.ExternalAgentWorkflow;
    }
}
