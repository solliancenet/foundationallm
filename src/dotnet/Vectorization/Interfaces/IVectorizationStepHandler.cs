using FoundationaLLM.Vectorization.Models;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Handles requests associated with a specific vectorization pipeline step.
    /// </summary>
    public interface IVectorizationStepHandler
    {
        /// <summary>
        /// The identifier of the vectorization pipeline step.
        /// </summary>
        string StepId { get; }

        /// <summary>
        /// Invokes the handler.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> for which the step should be handled.</param>
        /// <param name="state">The <see cref="VectorizationState"/> holding the state associated with the vectorization request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>True if the vectorization step request was handled successfully.</returns>
        Task<bool> Invoke(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken);
    }
}
