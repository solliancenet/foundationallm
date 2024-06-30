using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Base model type for AIModel resources
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(EmbeddingAIModel), AIModelTypes.Embedding)]
    [JsonDerivedType(typeof(AzureDataLakeDataSource), DataSourceTypes.AzureDataLake)]
    public class AIModelBase : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }
        /// <summary>
        /// The endpoint metadata needed to call the AI model endpoint
        /// </summary>
        public AIModelEndpoint? Endpoint { get; set; }
        /// <summary>
        /// The version for the AI model
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// Deployment name for the AI model
        /// </summary>
        public string? DeploymentName { get; set; }
        /// <summary>
        /// Key value parameters configured for the model
        /// </summary>
        public Dictionary<string, object> ModelParameters { get; set; } = new Dictionary<string, object>();

    }
}
