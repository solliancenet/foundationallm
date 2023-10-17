using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record DataSourceHubResponse
    {
        
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

        [JsonProperty("underlying_implementation")]
        public string? UnderlyingImplementation { get; set; }

        [JsonProperty("file_type")]
        public string? FileType { get; set; }

        [JsonProperty("authentication")]
        public Dictionary<string, string>? Authentication { get; set; }

        [JsonProperty("container")]
        public string? Container { get; set; }

        [JsonProperty("files")]
        public List<string>? Files { get; set; }

        [JsonProperty("dialect")]
        public string? Dialect { get; set; }

        [JsonProperty("include_tables")]
        public List<string>? IncludeTables { get; set; }

        [JsonProperty("exclude_tables")]
        public List<string>? ExcludeTables { get; set; }

        [JsonProperty("few_shot_example_count")]
        public int? FewShotExampleCount { get; set; }
    }
}