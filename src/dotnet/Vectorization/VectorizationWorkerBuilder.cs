using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Services;
using FoundationaLLM.Vectorization.Services.RequestSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FoundationaLLM.Vectorization
{
    /// <summary>
    /// Implements a builder for a vectorization worker.
    /// </summary>
    public class VectorizationWorkerBuilder
    {
        private VectorizationWorkerSettings? _settings;
        private IVectorizationStateService? _stateService;
        private CancellationToken _cancellationToken = default;
        private ILoggerFactory? _loggerFactory;
        private IConfigurationSection? _stepsConfiguration;
        private IServiceProvider? _serviceProvider;

        private readonly RequestSourcesBuilder _requestSourcesBuilder = new();

        /// <summary>
        /// Constructs a new instance of the builder.
        /// </summary>
        public VectorizationWorkerBuilder()
        {
        }

        /// <summary>
        /// Builds the vectorization worker.
        /// </summary>
        /// <returns>The vectorization worker instance.</returns>
        /// <exception cref="VectorizationException">Thrown if the state of the builder was not properly initialized.</exception>
        public VectorizationWorker Build()
        {
            if (_stateService == null)
                throw new VectorizationException("Cannot build a vectorization worker without a valid vectorization state service.");

            if (_settings == null)
                throw new VectorizationException("Cannot build a vectorization worker without valid settings.");

            if (_loggerFactory == null)
                throw new VectorizationException("Cannot build a vectorization worker without a valid logger factory.");

            if (_serviceProvider == null)
                throw new VectorizationException("Cannot build a vectorization worker without a valid DI service provider.");

            var requestSourceServices = _requestSourcesBuilder.Build();

            var requestManagerServices = _settings!.RequestManagers!
                .Select(rm => new RequestManagerService(
                    rm,
                    requestSourceServices,
                    _stateService,
                    _stepsConfiguration,
                    _serviceProvider,
                    _loggerFactory,
                    _cancellationToken))
                .ToList();

            var vectorizationWorker = new VectorizationWorker(
                requestManagerServices);

            return vectorizationWorker;
        }

        /// <summary>
        /// Specifies the vectorization state service that manages vectorization states.
        /// </summary>
        /// <param name="stateService">The <see cref="IVectorizationStateService"/> service managing state.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithStateService(IVectorizationStateService stateService)
        {
            _stateService = stateService;
            return this;
        }

        /// <summary>
        /// Specifies the settings used to build the vectorization worker.
        /// </summary>
        /// <param name="settings">The <see cref="VectorizationWorkerSettings"/>object providing the settings.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithSettings(VectorizationWorkerSettings settings) 
        {
            ValidateSettings(settings);

            _requestSourcesBuilder.WithSettings(settings!.RequestSources);
            _requestSourcesBuilder.WithQueuing(settings!.QueuingEngine);

            _settings = settings;
            return this;
        }

        /// <summary>
        /// Specifies the cancellation token used to signal stopping the vectorization worker.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to signal stopping.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        /// <summary>
        /// Specifies the logger factory used to create loggers for child objects.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
        {
            _requestSourcesBuilder.WithLoggerFactory(loggerFactory);

            _loggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Specifies the configuration section containing settings for the queues used by the vectorization worker.
        /// </summary>
        /// <param name="queuesConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithQueuesConfiguration(IConfigurationSection queuesConfiguration)
        {
            _requestSourcesBuilder.WithQueuesConfiguration(queuesConfiguration);
            return this;
        }

        /// <summary>
        /// Specifies the configuration section containing settings for vectorization pipeline steps.
        /// </summary>
        /// <param name="stepsConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithStepsConfiguration(IConfigurationSection stepsConfiguration)
        {
            _stepsConfiguration = stepsConfiguration;
            return this;
        }

        /// <summary>
        /// Specifies the dependency injection service provider.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
        /// <returns>The updated instance of the builder.</returns>
        public VectorizationWorkerBuilder WithServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }

        private static void ValidateSettings(VectorizationWorkerSettings settings)
        {
            if (
                settings == null 
                || settings.RequestManagers == null
                || settings.RequestManagers.Count == 0
                || settings.RequestSources == null)
                throw new ArgumentNullException(nameof(settings));

            foreach (var rm in settings.RequestManagers)
            {
                if (!VectorizationSteps.ValidateStepName(rm.RequestSourceName))
                    throw new VectorizationException("Configuration error: invalid request source name in RequestManagers.");

                if (!settings.RequestSources.Exists(rs => rs.Name.CompareTo(rm.RequestSourceName) == 0))
                    throw new VectorizationException($"Configuration error: RequestManagers references request source [{rm.RequestSourceName}] which is not configured.");
            }
        }
    }
}
