using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.ML.Tokenizers;

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Implements the <see cref="ITokenizer"/> interface.
    /// </summary>
    public class SemanticKernelTokenizer : ITokenizer
    {
        /// <summary>
        /// Gets the number of tokens for the input text.
        /// </summary>
        /// <param name="text">The text content.</param>
        /// <returns>The number of tokens.</returns>
        public int GetTokensCount(string text)
        {
            Tokenizer tokenizer = new(new Bpe());
            var tokens = tokenizer.Encode(text).Tokens;

            return tokens.Count;
        }
    }
}
