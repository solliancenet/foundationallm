using FoundationaLLM.Common.Settings;
using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the <see cref="SemanticKernelTextEmbeddingService"/> service.
    /// </summary>
    public record SemanticKernelTextEmbeddingServiceSettings
    {
        /// <summary>
        /// The name of the Azure Open AI deployment.
        /// </summary>
        public required string DeploymentName { get; set; }

        /// <summary>
        /// The endpoint of the Azure Open AI deployment.
        /// </summary>
        public required string Endpoint { get; set; }

        /// <summary>
        /// The API key used to connect to the Azure Open AI endpoint. Valid only if AuthenticationType is APIKey.
        /// </summary>
        public string? APIKey { get; set; }

        /// <summary>
        /// The <see cref="AuthenticationType"/> indicating which authentication mechanism to use.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AzureOpenAIAuthenticationTypes AuthenticationType {  get; set; } 
    }
}
