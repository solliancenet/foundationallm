namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    ///  Contains constants for the types of agent workflows.
    /// </summary>
    public static class AgentWorkflowTypes
    {
        /// <summary>
        /// The Azure AI Agent Service agent workflow.
        /// </summary>
        public const string AzureAIAgentService = "azure-ai-agent-service-workflow";

        /// <summary>
        /// The OpenAI Assistants agent workflow.
        /// </summary>
        public const string AzureOpenAIAssistants = "azure-openai-assistants-workflow";

        /// <summary>
        /// The LangChain Expression Language agent workflow.
        /// </summary>
        public const string LangChainExpressionLanguage = "langchain-expression-language-workflow";

        /// <summary>
        /// The LangGraph ReAct agent workflow.
        /// </summary>
        public const string LangGraphReactAgent = "langgraph-react-agent-workflow";

        /// <summary>
        /// The External Agent workflow.
        /// </summary>
        public const string ExternalAgentWorkflow = "external-agent-workflow";
    }
}
