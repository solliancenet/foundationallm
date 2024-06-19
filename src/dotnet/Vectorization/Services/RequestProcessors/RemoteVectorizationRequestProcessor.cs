using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Client;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.RequestProcessors
{
    /// <summary>
    /// Processes the vectorization request remotely using the <see cref="IVectorizationServiceClient"/> over HTTP.
    /// </summary>
    /// <param name="httpClientFactory">The factorory responsible for HTTP connections.</param>
    /// <param name="vectorizationServiceSettings">The settings for the vectorization service.</param>
    /// <param name="loggerFactory">The logger factory responsible for creating loggers.</param>
    public class RemoteVectorizationRequestProcessor(
        IHttpClientFactory httpClientFactory,
        IOptions<VectorizationServiceSettings> vectorizationServiceSettings,
        ILoggerFactory loggerFactory) : IVectorizationRequestProcessor
    {
        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            var vectorizationServiceClient = new VectorizationServiceClient(
                httpClientFactory,
                vectorizationServiceSettings!,
                loggerFactory.CreateLogger<VectorizationServiceClient>());
            return await vectorizationServiceClient.ProcessRequest(vectorizationRequest);
        }
    }
}
