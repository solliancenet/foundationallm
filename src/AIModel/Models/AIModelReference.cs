using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using System.Text.Json.Serialization;

namespace FoundationaLLM.AIModel.Models
{
    /// <summary>
    /// Contains a reference to an AIModel
    /// </summary>
    public class AIModelReference : ResourceReference
    {
        /// <summary>
        /// The object type of the data source.
        /// </summary>
        [JsonIgnore]
        public Type AIModelType =>
            Type switch
            {
                AIModelTypes.Basic => typeof(AIModelBase),
                AIModelTypes.Completion => typeof(CompletionAIModel),
                AIModelTypes.Embedding => typeof(EmbeddingAIModel),
                _ => throw new ResourceProviderException($"The data source type {Type} is not supported.")
            };
    }
}
