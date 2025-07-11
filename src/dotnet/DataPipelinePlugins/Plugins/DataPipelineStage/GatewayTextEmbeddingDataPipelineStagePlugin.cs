﻿using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Gateway Text Embedding Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class GatewayTextEmbeddingDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE;

        private const string CONTENT_PARTS_FILE_NAME = "content-parts.parquet";
        private const int GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS = 5;

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL,
                out var embeddingModel))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL} parameter.");

            if (!_pluginParameters.TryGetValue(
                PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                out var embeddingDimensions))
                throw new PluginException(
                    $"The plugin {Name} requires the {PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS} parameter.");

            var contentItemParts = await _dataPipelineStateService.LoadDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                CONTENT_PARTS_FILE_NAME);

            using var scope = _serviceProvider.CreateScope();

            var clientFactoryService = scope.ServiceProvider
                .GetRequiredService<IHttpClientFactoryService>()
                ?? throw new PluginException("The HTTP client factory service is not available in the dependency injection container.");

            var embeddingServiceClient = new GatewayServiceClient(
                await clientFactoryService.CreateClient(
                    HttpClientNames.GatewayAPI, ServiceContext.ServiceIdentity!),
                _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

            var textEmbeddingRequest = new TextEmbeddingRequest
            {
                EmbeddingModelName = embeddingModel.ToString()!,
                EmbeddingModelDimensions = (int)embeddingDimensions,
                Prioritized = false,
                TextChunks = [.. contentItemParts
                    .Select(part => new TextChunk
                    {
                        Position = part.Position,
                        Content = part.Content,
                        TokensCount = part.ContentSizeTokens
                    })]
            };

            var embeddingResult = await embeddingServiceClient.StartEmbeddingOperation(
                dataPipelineRun.InstanceId,
                textEmbeddingRequest);

            while (embeddingResult.InProgress)
            {
                await Task.Delay(TimeSpan.FromSeconds(GATEWAY_SERVICE_CLIENT_POLLING_INTERVAL_SECONDS));
                embeddingResult = await embeddingServiceClient.GetEmbeddingOperationResult(
                    dataPipelineRun.InstanceId,
                    embeddingResult.OperationId!);

                _logger.LogInformation("Data pipeline run {DataPipelineRunId} text parts embedding for {ContentItemCanonicalId}: {ProcessedEntityCount} of {TotalEntityCount} entities processed.",
                    dataPipelineRun.Id,
                    dataPipelineRunWorkItem.ContentItemCanonicalId,
                    embeddingResult.ProcessedTextChunksCount,
                    textEmbeddingRequest.TextChunks.Count);
            }

            if (embeddingResult.Failed)
                return new PluginResult(false, false,
                    $"The {Name} plugin failed to process the work item {dataPipelineRunWorkItem.Id} due to a failure in the Gateway API.");

            var embeddingsDictionary = embeddingResult.TextChunks.ToDictionary(
                chunk => chunk.Position,
                chunk => chunk.Embedding);

            foreach (var contentItemPart in contentItemParts)
            {
                if (embeddingsDictionary.TryGetValue(contentItemPart.Position, out var embedding))
                {
                    contentItemPart.Embedding = embedding!.Value.Vector.ToArray();
                }
                else
                    throw new PluginException($"The Gateway API did not return an embedding for the content item part at position {contentItemPart.Position}");
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemParts<DataPipelineContentItemContentPart>(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                contentItemParts,
                CONTENT_PARTS_FILE_NAME);

            return new PluginResult(true, false);
        }
    }
}
