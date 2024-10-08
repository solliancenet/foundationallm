﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the extraction stage of the vectorization pipeline.
    /// </summary>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class ExtractionHandler(
        string messageId,
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IVectorizationStateService stateService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(
            VectorizationSteps.Extract,
            messageId,
            parameters,
            stepsConfiguration,
            stateService,
            serviceProvider,
            loggerFactory)
    {
        /// <inheritdoc/>
        protected override async Task<bool> ProcessRequest(
            VectorizationRequest request,
            VectorizationState state,
            IConfigurationSection? stepConfiguration,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken)
        {
            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<IContentSourceService>>()
                ?? throw new VectorizationException($"Could not retrieve the content source service factory instance.");
            var contentSourceService = await serviceFactory.GetService(request.ContentIdentifier.DataSourceObjectId, userIdentity);

            var textContent = await contentSourceService.ExtractTextAsync(request.ContentIdentifier, userIdentity, cancellationToken);

            state.AddOrReplaceArtifact(new VectorizationArtifact
            {
                Type = VectorizationArtifactType.ExtractedText,
                Position = 1,
                Content = textContent,
                Size = textContent.Length
            });

            return true;
        }
    }
}
