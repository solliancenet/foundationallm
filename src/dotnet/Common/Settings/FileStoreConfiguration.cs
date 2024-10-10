using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// The configuration for the file store.
    /// </summary>
    public class FileStoreConfiguration
    {
        /// <summary>
        /// Indicates the maximum number of files that can be uploaded in a single message.
        /// </summary>
        public int MaxUploadsPerMessage { get; set; } = 10;
        /// <summary>
        /// A list of API endpoint configurations for file store connectors.
        /// </summary>
        public IEnumerable<APIEndpointConfiguration>? FileStoreConnectors { get; set; }
    }
}
