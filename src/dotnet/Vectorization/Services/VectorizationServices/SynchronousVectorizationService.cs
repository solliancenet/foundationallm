using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Extensions;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Services.VectorizationStates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.VectorizationServices
{
    /// <summary>
    /// Handles synchronous in-memory vectorization requests.
    /// </summary>
    /// <param name="vectorizationStateService"></param>
    /// <param name="resourceProviderServices"></param>
    /// <param name="stepsConfiguration"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public class SynchronousVectorizationService(       
        MemoryVectorizationStateService vectorizationStateService,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_Steps)]
        IConfigurationSection stepsConfiguration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationService
    {        
        private readonly MemoryVectorizationStateService _vectorizationStateService = vectorizationStateService;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly IConfigurationSection? _stepsConfiguration = stepsConfiguration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<AsynchronousVectorizationService> _logger = loggerFactory.CreateLogger<AsynchronousVectorizationService>();

        ///<inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            var vectorizationResourceProvider = GetVectorizationResourceProvider();
            vectorizationRequest.ProcessingState = VectorizationProcessingState.InProgress;
            vectorizationRequest.ExecutionStart = DateTime.UtcNow;
            await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);


            _logger.LogInformation("Starting synchronous processing for request {RequestId}.", vectorizationRequest.Name);

            var state = VectorizationState.FromRequest(vectorizationRequest);

            foreach (var step in vectorizationRequest.Steps)
            {
                _logger.LogInformation("Starting step [{Step}] for request {RequestId}.", step.Id, vectorizationRequest.Name);

                var stepHandler = VectorizationStepHandlerFactory.Create(
                    step.Id,
                "N/A",
                    step.Parameters,
                    _stepsConfiguration,
                    _vectorizationStateService,
                    _serviceProvider,
                    _loggerFactory);

                // vectorization request state is persisted in the Invoke method.
                var handlerSuccess = await stepHandler.Invoke(vectorizationRequest, state, default).ConfigureAwait(false);
                if (!handlerSuccess)
                    break;

                var steps = vectorizationRequest.MoveToNextStep();

                if (!string.IsNullOrEmpty(steps.CurrentStep))
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to step [{CurrentStepName}].",
                        vectorizationRequest.Name, steps.PreviousStep, steps.CurrentStep);
                else
                    _logger.LogInformation("The pipeline for request id {RequestId} was advanced from step [{PreviousStepName}] to finalized state.",
                       vectorizationRequest.Name, steps.PreviousStep);

                // save execution state
                await _vectorizationStateService.SaveState(state).ConfigureAwait(false);
            }

            if (vectorizationRequest.Complete)
            {
                // update the vectorization request state to Completed.
                vectorizationRequest.ProcessingState = VectorizationProcessingState.Completed;
                vectorizationRequest.ExecutionEnd = DateTime.UtcNow;
                await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);

                _logger.LogInformation("Finished synchronous processing for request {RequestId}. All steps were processed successfully.", vectorizationRequest.Name);
                return new VectorizationResult(vectorizationRequest.ObjectId!, true, null);
            }
            else
            {
                var errorMessage =
                    $"Execution stopped at step [{vectorizationRequest.CurrentStep}] due to an error.";

                // update the vectorization request state to Completed.
                vectorizationRequest.ProcessingState = VectorizationProcessingState.Failed;
                vectorizationRequest.ExecutionEnd = DateTime.UtcNow;
                await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider).ConfigureAwait(false);
                _logger.LogInformation("Finished synchronous processing for request {RequestId}. {ErrorMessage}", vectorizationRequest.Name, errorMessage);
                return new VectorizationResult(vectorizationRequest.ObjectId!, false, errorMessage);
            }
        }

        /// <summary>
        /// Obtains the vectorization resource provider from available resource providers.
        /// </summary>
        /// <returns>The vectorization resource provider</returns>
        /// <exception cref="VectorizationException"></exception>
        private IResourceProviderService GetVectorizationResourceProvider()
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");


            return vectorizationResourceProviderService;
        }
    }
}
