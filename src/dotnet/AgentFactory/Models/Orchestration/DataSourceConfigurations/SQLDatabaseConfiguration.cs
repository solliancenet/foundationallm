using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// SQL Database configuration settings.
    /// </summary>
    public class SQLDatabaseConfiguration
    {
        /// <summary>
        /// The SQL dialect
        /// </summary>
        [JsonProperty("dialect")]
        public string? Dialect { get; set; }

        /// <summary>
        /// The database server host name.
        /// </summary>
        [JsonProperty("host")]
        public string? Host { get; set; }

        /// <summary>
        /// The port number of the database on the host.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// The name of the database on the server.
        /// </summary>
        [JsonProperty("database_name")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// The username for connecting to the database.
        /// </summary>
        [JsonProperty("username")]
        public string? Username { get; set; }

        /// <summary>
        /// The name of the secret in Key Vault from where the password can be retrieved.
        /// </summary>
        [JsonProperty("password_secret_name")]
        public string? PasswordSecretName { get; set; }

        /// <summary>
        /// List of tables to allow access to in the database.
        /// </summary>
        [JsonProperty("include_tables")]
        public List<string> IncludeTables { get; set; } = new List<string>();

        /// <summary>
        /// The number of rows from each table to provide as examples to the language model.
        /// </summary>
        [JsonProperty("few_shot_example_count")]
        public int FewShotExampleCount { get; set; } = 0;
    }
}
