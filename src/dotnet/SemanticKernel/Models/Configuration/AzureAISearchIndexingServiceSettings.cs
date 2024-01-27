using FoundationaLLM.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the Azure AI Search indexing service.
    /// </summary>
    public record AzureAISearchIndexingServiceSettings
    {
        /// <summary>
        /// The endpoint of the Azure AI deployment.
        /// </summary>
        public required string Endpoint { get; set; }

        /// <summary>
        /// The API key used to connect to the Azure AI Search endpoint. Valid only if AuthenticationType is APIKey.
        /// </summary>
        public string? APIKey { get; set; }

        /// <summary>
        /// The <see cref="AuthenticationType"/> indicating which authentication mechanism to use.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AzureAISearchAuthenticationTypes AuthenticationType { get; set; }
    }
}
