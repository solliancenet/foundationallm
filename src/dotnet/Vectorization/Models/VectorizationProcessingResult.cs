using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents the result of processing a vectorization request.
    /// </summary>
    /// <param name="ObjectId">The object id to which the result refers to.</param>
    /// <param name="IsSuccess">Indicates whether the processing was completed successfully.</param>
    /// <param name="ErrorMessage">When IsSuccess is false, contains an error message with details.</param>
    public record VectorizationProcessingResult(
        [property: JsonPropertyName("object_id")] string ObjectId,
        [property: JsonPropertyName("is_success")] bool IsSuccess,
        [property: JsonPropertyName("error_message")] string? ErrorMessage)
    {
    }
}
