using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Models.Configuration.API
{
    /// <inheritdoc/>
    public record DownstreamAPISettings : IDownstreamAPISettings
    {
        /// <inheritdoc/>
        public required Dictionary<string, DownstreamAPIClientConfiguration> DownstreamAPIs { get; init; }
    }

    /// <summary>
    /// Represents settings for downstream API clients.
    /// </summary>
    public record DownstreamAPIClientConfiguration : APIClientSettings
    {
        /// <summary>
        /// The URL of the downstream API.
        /// </summary>
        public required string APIUrl { get; set; }
        /// <summary>
        /// The value of the API key.
        /// </summary>
        public required string APIKey { get; init; }
    }
}
