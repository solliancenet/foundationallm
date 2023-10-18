namespace FoundationaLLM.SemanticKernel.Core.Interfaces
{
    /// <summary>
    /// Interface for the Tokenizer service.
    /// </summary>
    public interface ITokenizer
    {
        /// <summary>
        /// Gets the number of tokens for the input text.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <returns>The number of tokens.</returns>
        int GetTokensCount(string text);
    }
}
