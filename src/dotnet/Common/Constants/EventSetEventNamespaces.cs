using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Namespace names for event set events.
    /// </summary>
    public static class EventSetEventNamespaces
    {
        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.Agent resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Agent = "ResourceProvider.FoundationaLLM.Agent";

        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Vectorization = "ResourceProvider.FoundationaLLM.Vectorization";

        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.Configuration resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Configuration = "ResourceProvider.FoundationaLLM.Configuration";

        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.DataSource resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_DataSource = "ResourceProvider.FoundationaLLM.DataSource";

        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.Attachment resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Attachment = "ResourceProvider.FoundationaLLM.Attachment";

        /// <summary>
        /// The namespace name for events concerning the FoundationaLLM.AIModel resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_AIModel = "ResourceProvider.FoundationaLLM.AIModel";
    }
}
