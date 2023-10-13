using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// CSV file configuration settings.
    /// </summary>
    public class CSVFileConfiguration
    {
        /// <summary>
        /// The location of the file.
        /// </summary>
        [JsonProperty("source_file_path")]
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Flag indicating whether the source file path is a secret value
        /// that must be looked up, or if it is a simple path that can be 
        /// passed in as is.
        /// </summary>
        [JsonProperty("path_value_is_secret")]
        public bool PathValueIsSecret { get; set; } = true;
    }
}
