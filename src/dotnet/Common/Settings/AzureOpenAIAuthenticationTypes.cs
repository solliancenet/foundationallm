using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// Types of authentication for Azure Open AI.
    /// </summary>
    public enum AzureOpenAIAuthenticationTypes
    {
        /// <summary>
        /// Unknown authentication type.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Azure managed identity authentication type.
        /// </summary>
        AzureIdentity,

        /// <summary>
        /// API key authentication type.
        /// </summary>
        APIKey
    }
}
