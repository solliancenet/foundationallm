using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization request that has been dequeued from a request source.
    /// </summary>
    public class VectorizationDequeuedRequest
    {
        /// <summary>
        /// The vectorization request.
        /// </summary>
        public required string RequestName { get; set;}

        /// <summary>
        /// The queue message identifier.
        /// </summary>
        public required string MessageId { get; set; }

        /// <summary>
        /// The queue pop receipt.
        /// </summary>
        public required string PopReceipt { get; set; }

        /// <summary>
        /// The number of times the message has been dequeued.
        /// </summary>
        public long DequeueCount { get; set; }
    }
}
