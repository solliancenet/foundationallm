using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    public static class AIModelTypes
    {
        /// <summary>
        /// Basic AIModel type without practical functionality. Used as base for all other model types.
        /// </summary>
        public const string Basic = "basic";
        /// <summary>
        /// Embedding model type
        /// </summary>
        public const string Embedding = "embedding";
        /// <summary>
        /// Completion model type
        /// </summary>
        public const string Completion = "completion";
    }
}
