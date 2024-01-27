using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Provides details about a resource type managed by a resource provider.
    /// </summary>
    /// <param name="resourceType">The name of the resource type.</param>
    public class ResourceTypeDescriptor(
        string resourceType)
    {
        /// <summary>
        /// The name of the resource type.
        /// </summary>
        public string ResourceType { get; set; } = resourceType;

        /// <summary>
        /// The list of actions supported by the resource type.
        /// </summary>
        public HashSet<string> Actions { get; set; } = [];

        /// <summary>
        /// The dictionary of resource descriptors specifying the resource's allowed subtypes.
        /// </summary>
        public Dictionary<string, ResourceTypeDescriptor> SubTypes { get; set; } = [];
    }
}
