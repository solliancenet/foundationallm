using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Authentication
{
    /// <inheritdoc/>
    public record DownstreamAPISettings : IDownstreamAPISettings
    {
        /// <inheritdoc/>
        public Dictionary<string, DownstreamAPIKeySettings> DownstreamAPIs { get; init; }
    }

    /// <summary>
    /// Represents settings for downstream API key authentication.
    /// </summary>
    public record DownstreamAPIKeySettings
    {
        /// <summary>
        /// The URL of the downstream API.
        /// </summary>
        public required string APIUrl { get; init; }
        /// <summary>
        /// The name of the secret that contains the API key.
        /// </summary>
        public required string APIKeySecretName { get; init; }
    }
}
