using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProvider;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Basic properties for vectorization profiles.
    /// </summary>
    public class VectorizationProfileBase
    {
        /// <summary>
        /// The name of the vectorization profile
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The unique identifier of the object.
        /// </summary>
        public string? ObjectId { get; set; }

        /// <summary>
        /// The configuration associated with the vectorization profile.
        /// </summary>
        public Dictionary<string, string>? Settings { get; set; } = [];

        /// <summary>
        /// Configuration references associated with the vectorizatio profile.
        /// </summary>
        public Dictionary<string, string>? ConfigurationReferences { get; set; } = [];
    }
}
