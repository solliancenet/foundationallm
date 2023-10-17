using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// Represents a response from the Data Source Hub.  Includes a list of datasources and their configuration information.
    /// </summary>
    public record DataSourceHubResponse
    {
        /// <summary>
        /// The list of data sources returned from a DataSource Hub request.
        /// </summary>
        [JsonProperty("data_sources")]
        public List<DataSourceMetadata>? DataSources { get; set; }

    }

    /// <summary>
    /// SQL DataSource 
    /// </summary>
    public record SQLDataSourceMetadata : DataSourceMetadata
    {

    }

    /// <summary>
    /// Blob Storage DataSource 
    /// </summary>
    public record BlobStorageDataSourceMetadata : DataSourceMetadata
    {

    }


    /// <summary>
    /// Default set of properties for a DataSource returned from DataSource Hub.
    /// </summary>
    public record DataSourceMetadata
    {
        /// <summary>
        /// Name of data source
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Description of data source
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>
        /// Type of data source
        /// </summary>
        [JsonProperty("underlying_implementation")]
        public string? UnderlyingImplementation { get; set; }

        /// <summary>
        /// File type of data source
        /// </summary>
        [JsonProperty("file_type")]
        public string? FileType { get; set; }

        /// <summary>
        /// Authentication details for the data source.
        /// </summary>
        [JsonProperty("authentication")]
        public Dictionary<string, string>? Authentication { get; set; }

        /// <summary>
        /// For blob storage, the container to search
        /// </summary>
        [JsonProperty("container")]
        public string? Container { get; set; }

        /// <summary>
        /// For blob storage, the files to query.
        /// </summary>
        [JsonProperty("files")]
        public List<string>? Files { get; set; }

        /// <summary>
        /// The dialect of the data source (SQL, etc)
        /// </summary>
        [JsonProperty("dialect")]
        public string? Dialect { get; set; }

        /// <summary>
        /// For a SQL data source, the tables to include for processing.
        /// </summary>
        [JsonProperty("include_tables")]
        public List<string>? IncludeTables { get; set; }

        /// <summary>
        /// For a SQL data source, the tables to exclude from processing.
        /// </summary>
        [JsonProperty("exclude_tables")]
        public List<string>? ExcludeTables { get; set; }

        /// <summary>
        /// List of few shot examples for the prompt processing.
        /// </summary>
        [JsonProperty("few_shot_example_count")]
        public int? FewShotExampleCount { get; set; }
    }
}