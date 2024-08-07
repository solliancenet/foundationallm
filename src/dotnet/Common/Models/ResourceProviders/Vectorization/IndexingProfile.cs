using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides details about an indexing profile.
    /// </summary>
    public class IndexingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the indexer.
        /// </summary>
        [JsonPropertyName("indexer")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required IndexerType Indexer { get; set; }

        /// <summary>
        /// The object id of the service used for indexing.
        /// </summary>
        [JsonPropertyName("indexing_api_endpoint_configuration_object_id")]
        public required string IndexingAPIEndpointConfigurationObjectId { get; set; }
    }
}
