using Microsoft.SemanticKernel.Connectors.AI.OpenAI.Tokenizers;

namespace Solliance.AICopilot.SemanticKernel.Chat
{
    public class SemanticKernelTokenizer : ITokenizer
    {
        public int GetTokensCount(string text)
        {
            return GPT3Tokenizer.Encode(text).Count;
        }
    }
}
