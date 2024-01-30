namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings to initialize a Sql Database content source service.
    /// </summary>
    public class AzureSQLDatabaseContentSourceServiceSettings
    {
        /// <summary>
        /// The connection string used for authentication.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}
