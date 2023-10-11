using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Represents settings for downstream API key authentication.
    /// </summary>
    public record DownstreamAPIKeySettings
    {
        /// <summary>
        /// The name of the secret that contains the API key.
        /// </summary>
        public required string APIKeySecretName { get; set; }
    }
}
