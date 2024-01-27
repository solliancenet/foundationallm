using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.Text
{
    /// <summary>
    /// Provides configuration settings that control token-based text splitting.
    /// </summary>
    /// <param name="Tokenizer">The tokenizer used to split the test into tokens.</param>
    /// <param name="TokenizerEncoder">The name of the encoder used for tokenization.</param>
    /// <param name="ChunkSizeTokens">The target size in tokens for the resulting text chunks.</param>
    /// <param name="OverlapSizeTokens">Teh target size in tokens for the overlapping parts of the adjacent text chunks.</param>
    public record TokenTextSplitterServiceSettings(
        string Tokenizer,
        string TokenizerEncoder,
        int ChunkSizeTokens,
        int OverlapSizeTokens)
    {
        /// <summary>
        /// Creates and instance of the class based on a dictionary.
        /// </summary>
        /// <param name="settings">The dictionary containing the settings.</param>
        /// <returns>A <see cref="TokenTextSplitterServiceSettings"/> instance initialized with the values from the dictionary.</returns>
        public static TokenTextSplitterServiceSettings FromDictionary(Dictionary<string, string> settings)
        {
            if (settings.TryGetValue("Tokenizer", out var tokenizer)
                && settings.TryGetValue("TokenizerEncoder", out var tokenizerEncoder)
                && settings.TryGetValue("ChunkSizeTokens", out var chunkSizeTokens)
                && settings.TryGetValue("OverlapSizeTokens", out var overlapSizeTokens))
                return new TokenTextSplitterServiceSettings(
                    tokenizer,
                    tokenizerEncoder,
                    int.Parse(chunkSizeTokens),
                    int.Parse(overlapSizeTokens));

            throw new TextProcessingException("Invalid text splitter settings.");
        }
    }
}
