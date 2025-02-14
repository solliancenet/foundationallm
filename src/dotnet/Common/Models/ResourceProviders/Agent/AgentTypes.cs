namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Contains constants for the types of resources managed by the FoundationaLLM.Agent resource provider.
    /// </summary>
    public static class AgentTypes
    {
        /// <summary>
        /// Basic agent without practical functionality. Used as base for all the other agents.
        /// </summary>
        public const string Basic = "basic";

        /// <summary>
        /// Knowledge Management agents are best for Q&amp;A, summarization, and reasoning over textual data.
        /// </summary>
        public const string KnowledgeManagement = "knowledge-management";

        /// <summary>
        /// Agent file.
        /// </summary>
        public const string AgentFile = "agent-file";


        /// <summary>
        /// Workflow.
        /// </summary>
        public const string Workflow = "workflow";

        /// <summary>
        /// Tool.
        /// </summary>
        public const string Tool = "tool";
    }
}
