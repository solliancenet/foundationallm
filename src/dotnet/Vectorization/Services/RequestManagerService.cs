using FoundationaLLM.Common.Tasks;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Services
{
    /// <summary>
    /// Manages vectorization requests originating from a specific request source.
    /// </summary>
    public class RequestManagerService : IRequestManagerService
    {
        private readonly RequestManagerServiceSettings _settings;
        private readonly Dictionary<string, IRequestSourceService> _requestSourceServices;
        private readonly IRequestSourceService _incomingRequestSourceService;
        private readonly IVectorizationStateService _vectorizationStateService;
        private readonly IConfigurationSection? _stepsConfiguration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RequestManagerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskPool _taskPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestManagerService"/> class with the configuration and services
        /// required to manage vectorization requests originating from a specific request source.
        /// </summary>
        /// <param name="settings">The configuration settings used to initialize the instance.</param>
        /// <param name="requestSourceServices">The dictionary with all the request source services registered in the vectorization platform.</param>
        /// <param name="vectorizationStateService">The service providing vectorization state management.</param>
        /// <param name="stepsConfiguration">The <see cref="IConfigurationSection"/> object providing access to the settings.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the work.</param>
        /// <exception cref="VectorizationException">The exception thrown when the initialization of the instance fails.</exception>
        public RequestManagerService(
            RequestManagerServiceSettings settings,
            Dictionary<string, IRequestSourceService> requestSourceServices,
            IVectorizationStateService vectorizationStateService,
            IConfigurationSection? stepsConfiguration,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken)
        {
            _settings = settings;
            _requestSourceServices = requestSourceServices;
            _vectorizationStateService = vectorizationStateService;
            _stepsConfiguration = stepsConfiguration;

            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<RequestManagerService>();
            _cancellationToken = cancellationToken;

            if (!_requestSourceServices.TryGetValue(_settings.RequestSourceName, out IRequestSourceService? value) || value == null)
                throw new VectorizationException($"Could not find a request source service for [{_settings.RequestSourceName}].");

            _incomingRequestSourceService = value;

            _taskPool = new TaskPool(_settings.MaxHandlerInstances);
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] started processing requests.", _settings.RequestSourceName);

            while (true)
            {
                if (_cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var taskPoolAvailableCapacity = _taskPool.AvailableCapacity;

                    if (taskPoolAvailableCapacity > 0 && (await _incomingRequestSourceService.HasRequests().ConfigureAwait(false)))
                    {
                        var requests = await _incomingRequestSourceService.ReceiveRequests(taskPoolAvailableCapacity).ConfigureAwait(false);

                        // No need to use ConfigureAwait(false) since the code is going to be executed on a
                        // thread pool thread, with no user code higher on the stack (for details, see
                        // https://devblogs.microsoft.com/dotnet/configureawait-faq/).
                        _taskPool.Add(
                            requests.Select(r => Task.Run(
                                () => { ProcessRequest(r.Request, r.MessageId, r.PopReceipt, _cancellationToken).ConfigureAwait(false); },
                                _cancellationToken)));
                    }

                    // Wait a predefined amount of time before attempting to receive requests again.
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in request processing loop (request source name: {RequestSourceName}).", _settings.RequestSourceName);
                }
            }

            _logger.LogInformation("The request manager service associated with source [{RequestSourceName}] finished processing requests.", _settings.RequestSourceName);
        }

        private async Task ProcessRequest(VectorizationRequest request, string messageId, string popReceipt, CancellationToken cancellationToken)
        {
            try
            {
                if (await HandleRequest(request, messageId, cancellationToken).ConfigureAwait(false))
                {
                    // If the request was handled successfully, remove it from the current source and advance it to the next step.
                    await _incomingRequestSourceService.DeleteRequest(messageId, popReceipt).ConfigureAwait(false);
                    await AdvanceRequest(request).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request with id {RequestId}.", request.Id);
            }
        }

        private async Task<bool> HandleRequest(VectorizationRequest request, string messageId, CancellationToken cancellationToken)
        {
            var state = await _vectorizationStateService.HasState(request).ConfigureAwait(false)
                ? await _vectorizationStateService.ReadState(request).ConfigureAwait(false)
                : VectorizationState.FromRequest(request);

            var stepHandler = VectorizationStepHandlerFactory.Create(
                _settings.RequestSourceName,
                messageId,
                request[_settings.RequestSourceName]!.Parameters,
                _stepsConfiguration,
                _vectorizationStateService,
                _serviceProvider,
                _loggerFactory);
            var handlerSuccess = await stepHandler.Invoke(request, state, cancellationToken).ConfigureAwait(false);

            await _vectorizationStateService.SaveState(state).ConfigureAwait(false);

            return handlerSuccess;
        }

        private async Task AdvanceRequest(VectorizationRequest request)
        {
            var steps = request.MoveToNextStep();

            if (!string.IsNullOrEmpty(steps.CurrentStep))
            {
                // The vectorization request still has steps to be processed
                if (!_requestSourceServices.TryGetValue(steps.CurrentStep, out IRequestSourceService? value) || value == null)
                    throw new VectorizationException($"Could not find the [{steps.CurrentStep}] request source service for request id {request.Id}.");

                await value.SubmitRequest(request).ConfigureAwait(false);

                _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to step [{CurrentStepName}].",
                    request.Id, steps.PreviousStep, steps.CurrentStep);
            }
            else
                _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to finalized state.",
                    request.Id, steps.PreviousStep);
        }
    }
}
