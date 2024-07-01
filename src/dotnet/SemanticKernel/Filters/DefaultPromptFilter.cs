using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0001

namespace FoundationaLLM.SemanticKernel.Core.Filters
{
    /// <summary>
    /// Provides the default behavior for filtering actions during prompt rendering.
    /// </summary>
    public class DefaultPromptFilter : IPromptRenderFilter
    {
        /// <summary>
        /// The rendered prompt.
        /// </summary>
        public string RenderedPrompt => _renderedPrompt;

        private string _renderedPrompt = string.Empty;       
        
        /// <inheritdoc/>
        public Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            _renderedPrompt = context.RenderedPrompt ?? string.Empty;
            return next(context);
        }
    }
}
