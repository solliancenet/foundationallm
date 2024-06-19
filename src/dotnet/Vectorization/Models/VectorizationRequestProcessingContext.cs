using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization request processing context.
    /// This class associates a dequeued request with the corresponding vectorization request resource.
    /// </summary>
    public class VectorizationRequestProcessingContext
    {
        /// <summary>
        /// The message that was dequeued.
        /// </summary>
        public required VectorizationDequeuedRequest DequeuedRequest { get; set; }

        /// <summary>
        /// The vectorization request resource.
        /// </summary>
        public required VectorizationRequest Request { get; set; }
    }
}
