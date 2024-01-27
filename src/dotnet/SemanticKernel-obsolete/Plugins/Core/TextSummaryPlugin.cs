using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;

namespace FoundationaLLM.SemanticKernel.Core.Plugins.Core
{
    /// <summary>
    /// Text summarizer plugin for Semantic Kernel.
    /// </summary>
    public class TextSummaryPlugin
    {
        private readonly ISKFunction _summarizeConversation;
        private readonly IKernel _kernel;

        /// <summary>
        /// Constructor for the Text Summary Plugin.
        /// </summary>
        /// <param name="promptTemplate">The prompt template.</param>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <param name="kernel">The Semantic Kernel instance.</param>
        public TextSummaryPlugin(
            string promptTemplate,
            int maxTokens,
            IKernel kernel)
        {
            _kernel = kernel;
            _summarizeConversation = kernel.CreateSemanticFunction(
                promptTemplate,
                pluginName: nameof(TextSummaryPlugin),
                description: "Given a text, summarize the text.",
                requestSettings: new OpenAIRequestSettings
                {
                    MaxTokens = maxTokens,
                    Temperature = 0.1,
                    TopP = 0.5
                });
        }

        /// <summary>
        /// Gets a summary from the Semantic Kernel service.
        /// </summary>
        /// <param name="text">The user prompt text.</param>
        /// <returns>The prompt summary.</returns>
        [SKFunction]
        public async Task<string> SummarizeTextAsync(
            string text)
        {
            var result = await _kernel.RunAsync(text, _summarizeConversation);
            return result.GetValue<string>() ?? string.Empty;
        }
    }
}
