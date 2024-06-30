using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Embedding AI Model
    /// </summary>
    public class EmbeddingAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EmbeddingAIModel"/> AI model.
        /// </summary>
        public EmbeddingAIModel() =>
            Type = AIModelTypes.Embedding;
    }
}
