using FoundationaLLM.Vectorization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Interface for the vectorization service.
    /// </summary>
    public interface IVectorizationService
    {
        /// <summary>
        /// Processes an incoming vectorization request.
        /// </summary>
        /// <param name="vectorizationRequest">The <see cref="VectorizationRequest"/> object containing the details of the vectorization request.</param>
        /// <returns></returns>
        Task<VectorizationProcessingResult> ProcessRequest(VectorizationRequest vectorizationRequest);
    }
}
