using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Identifies a specific resource type instance.
    /// </summary>
    /// <param name="ResourceType">The name of the resource type.</param>
    public record ResourceTypeInstance(
        string ResourceType)
    {
        /// <summary>
        /// An optional resource type instance unique identifier.
        /// </summary>
        public string? ResourceId;

        /// <summary>
        /// An optional action to be executed on the resource instance.
        /// </summary>
        public string? Action;
    }
}
