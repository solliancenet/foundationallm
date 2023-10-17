using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// SettingsBase class
    /// </summary>
    public class SettingsBase
    {
        /// <summary>
        /// The uri of the target service.
        /// </summary>
        public string? APIUrl { get; init; }

        /// <summary>
        /// The key that must be sent or validated against when sending requests to the service.
        /// </summary>
        public string? APIKey { get; init; }
    }
}
