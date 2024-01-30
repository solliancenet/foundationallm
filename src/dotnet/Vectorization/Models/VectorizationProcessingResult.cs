using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents the result of processing a vectorization request.
    /// </summary>
    /// <param name="IsSuccess">Indicates whether the processing was completed successfully.</param>
    /// <param name="OperationId">The identifier of the vectorization operation. Can be used to request the status of the operation.</param>
    /// <param name="ErrorMessage">When IsSuccess is false, contains an error message with details.</param>
    public record VectorizationProcessingResult(
        bool IsSuccess,
        Guid? OperationId,
        string? ErrorMessage)
    {
    }
}
