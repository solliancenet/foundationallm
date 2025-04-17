namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides the agent tool categories.
    /// </summary>
    public static class AgentToolCategories
    {
        /// <summary>
        /// The knowledge search category of tools.
        /// </summary>
        public const string KnowledgeSearch = "Knowledge Search";

        /// <summary>
        /// The code interpreter category of tools.
        /// </summary>
        public const string CodeInterpreter = "Code Interpreter";

        /// <summary>
        /// The image generation category of tools.
        /// </summary>
        public const string ImageGeneration = "Image Generation";

        /// <summary>
        /// The generic category of tools. Contains tools that are not part of any other category.
        /// </summary>
        public const string Generic = "Generic";
    }
}
