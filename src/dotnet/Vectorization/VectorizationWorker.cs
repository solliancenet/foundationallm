using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization
{
    /// <summary>
    /// Provides the core execution context for the vectorization worker.
    /// </summary>
    /// <param name="requestManagerServices">The collection of request managers to run.</param>
    public class VectorizationWorker(
        IEnumerable<IRequestManagerService> requestManagerServices)
    {
        private readonly IEnumerable<IRequestManagerService> _requestManagerServices = requestManagerServices;

        /// <summary>
        /// Starts all the request managers and enters a holding pattern waiting on them to stop.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var requestManagerTasks = _requestManagerServices
                .Select(rms => Task.Run(() => rms.Run()))
                .ToArray();

            await Task.WhenAll(requestManagerTasks);
        }
    }
}
