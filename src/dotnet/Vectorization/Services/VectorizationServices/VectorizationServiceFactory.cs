using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.VectorizationServices
{
    /// <summary>
    /// Responsible for creating the appropriate vectorization service based on the request processing type.
    /// </summary>
    /// <param name="requestSourcesCache"></param>
    /// <param name="inMemoryStateService">The In-Memory Vectorization State Service for Synchronous requests.</param>
    /// <param name="resourceProviderServices">The collection of configured Resource Providers.</param>
    /// <param name="stepsConfiguration">The configuration of different steps of the pipeline.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="loggerFactory">The factory responsible for logging.</param>
    public class VectorizationServiceFactory(
        IRequestSourcesCache requestSourcesCache,
        MemoryVectorizationStateService inMemoryStateService,        
        IEnumerable<IResourceProviderService> resourceProviderServices,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps)] IConfigurationSection stepsConfiguration,
        IOptions<InstanceSettings> instanceSettings,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {       
        /// <summary>
        /// Creates the appropriate vectorization service based on the request processing type.
        /// </summary>
        /// <param name="request">The vectorization request.</param>
        /// <returns>The vectorization service that will process the request.</returns>
        public IVectorizationService GetService(VectorizationRequest request)
        {
            if (request.ProcessingType == VectorizationProcessingType.Synchronous)
            {
                return new SynchronousVectorizationService(
                    inMemoryStateService,
                    resourceProviderServices,
                    stepsConfiguration,
                    instanceSettings.Value,
                    serviceProvider,
                    loggerFactory);                    
            }
            else
            {
                return new AsynchronousVectorizationService(requestSourcesCache);
            }
        }
    }
}
