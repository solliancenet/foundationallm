using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Implements a factory that creates vectorization step handlers.
    /// </summary>
    public class VectorizationStepHandlerFactory
    {
        /// <summary>
        /// Creates a vectorization step handler capable of handling a specified vectorization pipeline step.
        /// </summary>
        /// <param name="step">The identifier of the vectorization pipeline step for which the handler is created.</param>
        /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
        /// <param name="parameters">The parameters used to initialize the vectorization step handler.</param>
        /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
        /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <returns>A class implementing <see cref="IVectorizationStepHandler"/>.</returns>
        public static IVectorizationStepHandler Create(
            string step,
            string messageId,
            Dictionary<string, string> parameters,
            IConfigurationSection? stepsConfiguration,
            IVectorizationStateService stateService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory) =>
            step switch
            {
                VectorizationSteps.Extract => new ExtractionHandler(messageId, parameters, stepsConfiguration, stateService, serviceProvider, loggerFactory),
                VectorizationSteps.Partition => new PartitionHandler(messageId, parameters, stepsConfiguration, stateService, serviceProvider, loggerFactory),
                VectorizationSteps.Embed => new EmbeddingHandler(messageId, parameters, stepsConfiguration, stateService, serviceProvider, loggerFactory),
                VectorizationSteps.Index => new IndexingHandler(messageId, parameters, stepsConfiguration, stateService, serviceProvider, loggerFactory),
                _ => throw new VectorizationException($"There is no handler available for the vectorization pipeline step [{step}]."),
            };
    }
}
