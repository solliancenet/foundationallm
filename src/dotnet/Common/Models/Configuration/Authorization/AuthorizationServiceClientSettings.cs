namespace FoundationaLLM.Common.Models.Configuration.Authorization
{
    /// <summary>
    /// Authorization service client settings
    /// </summary>
    public record AuthorizationServiceClientSettings
    {
        /// <summary>
        /// Provides the API URL of the Authorization service.
        /// </summary>
        public required string APIUrl { get; set; }

        /// <summary>
        /// Provides the API scope of the Authorization service.
        /// </summary>
        public required string APIScope { get; set; }

        /// <summary>
        /// Indicates whether to use caching in the Authorization service client.
        /// </summary>
        public bool EnableCache { get; set; } = false;
    }
}
