using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// Basic properties for all resources.
    /// </summary>
    public class ResourceBase
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonProperty("name", Order = -5)]
        public required string Name { get; set; }
        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonProperty("type", Order = -4)]
        public required string Type { get; set; }
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonProperty("object_id", Order = -3)]
        public required string ObjectId { get; set; }
        /// <summary>
        /// The description of the resource.
        /// </summary>
        [JsonProperty("description", Order = -2)]
        public string? Description { get; set; }
    }
}
