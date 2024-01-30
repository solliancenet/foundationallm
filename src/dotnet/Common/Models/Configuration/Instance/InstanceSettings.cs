using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.Instance
{
    /// <summary>
    /// Provides configuration settings for the current FoundationaLLM deployment instance.
    /// </summary>
    public class InstanceSettings
    {
        /// <summary>
        /// The unique identifier of the current FoundationaLLM deployment instance.
        /// Format is a GUID.
        /// </summary>
        public required string Id { get; set; }
    }
}
