using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record DataSourceHubResponse
    {
        //[JsonObject]
        [JsonProperty("data_sources")]
        public List<DataSourceMetadata>? DataSources { get; set; }

    }

    public record SQLDataSourceMetadata : DataSourceMetadata
    {

    }

    public record BlobStorageDataSourceMetadata : DataSourceMetadata
    {

    }

    public record DataSourceMetadata
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }

    public record UnderlyingImplementation
    {

    }
}