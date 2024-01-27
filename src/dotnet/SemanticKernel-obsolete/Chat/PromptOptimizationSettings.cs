namespace FoundationaLLM.SemanticKernel.Chat
{
    /// <summary>
    /// Provides configuration options for prompt optimization.
    /// </summary>
    public record PromptOptimizationSettings
    {
        /// <summary>
        /// Minimum number of tokens for Completions.
        /// </summary>
        public required int CompletionsMinTokens { get; init; }

        /// <summary>
        /// Maximum number of tokens for Completions.
        /// </summary>
        public required int CompletionsMaxTokens { get; init; }

        /// <summary>
        /// Maximum number of tokens for System.
        /// </summary>
        public required int SystemMaxTokens { get; init; }

        /// <summary>
        /// Minimum number of tokens for Memory.
        /// </summary>
        public required int MemoryMinTokens { get; init; }

        /// <summary>
        /// Maximum number of tokens for Memory.
        /// </summary>
        public required int MemoryMaxTokens { get; init; }

        /// <summary>
        /// Minimum number of tokens for Messages.
        /// </summary>
        public required int MessagesMinTokens { get; init; }

        /// <summary>
        /// Maximum number of tokens for Messages.
        /// </summary>
        public required int MessagesMaxTokens { get; init; }
    }
}
