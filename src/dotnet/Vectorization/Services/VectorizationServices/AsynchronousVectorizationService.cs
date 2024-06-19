using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;

namespace FoundationaLLM.Vectorization.Services.VectorizationServices
{
    /// <summary>
    /// Implements the <see cref="IVectorizationService"/> interface for Asynchronous vectorization requests.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="AsynchronousVectorizationService"/> service.
    /// </remarks>
    /// <param name="requestSourcesCache">The <see cref="IRequestSourcesCache"/> cache of request sources.</param>   
    public class AsynchronousVectorizationService(
        IRequestSourcesCache requestSourcesCache) : IVectorizationService
    {
        private readonly Dictionary<string, IRequestSourceService> _requestSources = requestSourcesCache.RequestSources;       

        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            var firstRequestSource = _requestSources[vectorizationRequest.Steps.First().Id];
            await firstRequestSource.SubmitRequest(vectorizationRequest.Name);
            return new VectorizationResult(vectorizationRequest.ObjectId!, true, null);           
        }

    }
}
