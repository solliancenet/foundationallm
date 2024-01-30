using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// Represents the result of an upsert operation.
    /// </summary>
    public class ResourceProviderUpsertResult
    {
        /// <summary>
        /// The id of the object that was created or updated.
        /// </summary>
        public string? ObjectId { get; set; }
    }
}
