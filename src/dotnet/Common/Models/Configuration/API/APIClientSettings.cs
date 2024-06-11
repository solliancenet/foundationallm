using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.API
{
    /// <summary>
    /// Standard settings for an API client.
    /// </summary>
    public record APIClientSettings
    {
        /// <summary>
        /// The URL of the downstream API.
        /// </summary>
        public required string APIUrl { get; set; }
        /// <summary>
        /// Specifies the timeout for the downstream API HTTP client.
        /// If this value is null, the default timeout is used.
        /// For an infinite waiting period, use <see cref="Timeout.InfiniteTimeSpan"/>
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
