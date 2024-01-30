using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// The type of vectorization request processing.
    /// </summary>
    public enum VectorizationProcessingType
    {
        /// <summary>
        /// Asynchronous processing using vectorization workers.
        /// </summary>
        Asynchronous,

        /// <summary>
        /// Synchronous processing using the vectorization API.
        /// </summary>
        Synchronous
    }
}
