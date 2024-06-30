using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Completion AI Model
    /// </summary>
    public class CompletionAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CompletionAIModel"/> AI model.
        /// </summary>
        public CompletionAIModel() =>
            Type = AIModelTypes.Completion;
    }
}
