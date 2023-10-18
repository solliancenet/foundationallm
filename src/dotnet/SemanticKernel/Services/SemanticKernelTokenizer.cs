using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;

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
            return GPT3Tokenizer.Encode(text).Count;
        }
    }
}
