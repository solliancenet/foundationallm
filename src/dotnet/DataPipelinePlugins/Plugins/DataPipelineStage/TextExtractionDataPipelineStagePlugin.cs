﻿using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Plugins;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage
{
    /// <summary>
    /// Implements the Text Extraction Data Pipeline Stage Plugin.
    /// </summary>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class TextExtractionDataPipelineStagePlugin(
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IServiceProvider serviceProvider)
        : DataPipelineStagePluginBase(pluginParameters, packageManager, serviceProvider)
    {
        protected override string Name => PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE;

        private readonly Dictionary<string, string> _contentTypeMappings = new()
        {
            { "application/pdf", "PDF" },
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "DOCX" },
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", "PPTX" },
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "XLSX" },
            { "text/plain", "TXT" },
            { "text/markdown", "MD" },
            { "text/html", "HTML" },
            { "application/json", "JSON" },
            { "application/x-ndjson", "JSONL" },
            { "application/jsonlines", "JSONL" },
            { "application/xml", "XML" },
            { "text/csv", "CSV" },
            { "application/zip", "ZIP" },
            { "image/jpeg", "JPEG" },
            { "image/png", "PNG" },
            { "image/gif", "GIF" },
            { "image/bmp", "BMP" },
            { "image/tiff", "TIFF" }
        };

        /// <inheritdoc/>
        public override async Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(
            List<DataPipelineContentItem> contentItems,
            string dataPipelineRunId,
            string dataPipelineStageName)
        {
            var workItems = contentItems
                .Select(ci => new DataPipelineRunWorkItem
                {
                    Id = $"work-item-{Guid.NewGuid().ToBase64String()}",
                    RunId = dataPipelineRunId,
                    Stage = dataPipelineStageName,
                    ContentItemCanonicalId = ci.ContentIdentifier.CanonicalId
                })
                .ToList();

            return await Task.FromResult(workItems);
        }

        /// <inheritdoc/>
        public override async Task<PluginResult> ProcessWorkItem(
            DataPipelineDefinition dataPipelineDefinition,
            DataPipelineRun dataPipelineRun,
            DataPipelineRunWorkItem dataPipelineRunWorkItem)
        {
            if (!string.IsNullOrWhiteSpace(dataPipelineRunWorkItem.PreviousStage))
                throw new PluginException(
                    $"The plugin {Name} can only be used for data pipeline starting stages.");

            var dataSourcePluginName = ResourcePath.GetResourcePath(
                dataPipelineDefinition.DataSource.PluginObjectId).ResourceId;

            var dataSourcePlugin =
                _packageManager.GetDataSourcePlugin(
                    dataSourcePluginName!,
                    dataPipelineDefinition.DataSource.DataSourceObjectId,
                    dataPipelineRun.TriggerParameterValues.FilterKeys(
                        $"DataSource.{dataPipelineDefinition.DataSource.Name}."),
                    _serviceProvider);

            var rawContentResult = await dataSourcePlugin.GetContentItemRawContent(
                dataPipelineRunWorkItem.ContentItemCanonicalId);

            if (!rawContentResult.Success)
                return new PluginResult(false, false, rawContentResult.ErrorMessage);

            if (!_contentTypeMappings.TryGetValue(
                rawContentResult.Value!.ContentType,
                out var contentType))
            {
                return new PluginResult(false, false,
                    $"The content type {rawContentResult.Value.ContentType} is not supported by the {Name} plugin.");
            }

            var textContent = string.Empty;

            switch (contentType)
            {
                case "PDF":
                case "DOCX":
                case "PPTX":
                case "XLSX":

                    // Find all text extraction plugins that support the content type
                    var textExtractionPluginsMetadata = _packageManager
                        .GetMetadata(dataPipelineRun.InstanceId)
                        .Plugins
                        .Where(p =>
                            p.Category == PluginCategoryNames.ContentTextExtraction
                            && (p.Subcategory?.Split(',').Contains(contentType) ?? false))
                        .ToList();

                    if (textExtractionPluginsMetadata.Count == 0)
                        throw new PluginException(
                            $"The {Name} plugin cannot map the content type {contentType} to a content text extraction plugin.");

                    var dataPipelineStage = dataPipelineDefinition.GetStage(
                        dataPipelineRunWorkItem.Stage);

                    // Find the first plugin dependency that supports the content type
                    var pluginDependency = dataPipelineStage.PluginDependencies
                        .FirstOrDefault(pd => textExtractionPluginsMetadata.Select(p => p.ObjectId).Contains(pd.PluginObjectId))
                        ?? throw new PluginException(
                            $"The {dataPipelineRunWorkItem.Stage} stage does not have a dependency content text extraction plugin to handle the {contentType} content type.");

                    var textExtractionPluginMetadata = textExtractionPluginsMetadata
                        .Single(p => p.ObjectId == pluginDependency.PluginObjectId);

                    var textExtractionPlugin = _packageManager
                        .GetContentTextExtractionPlugin(
                            textExtractionPluginMetadata.Name,
                            dataPipelineRun.TriggerParameterValues.FilterKeys(
                                $"Stage.{dataPipelineRunWorkItem.Stage}.Dependency.{textExtractionPluginMetadata.Name.Split('-').Last()}."),
                            _serviceProvider);

                    var textContentResult = await textExtractionPlugin.ExtractText(
                        rawContentResult.Value.RawContent);

                    if (!textContentResult.Success)
                        return new PluginResult(false, false, textContentResult.ErrorMessage);

                    textContent = textContentResult.Value!;

                    break;

                case "TXT":
                case "MD":
                case "HTML":

                    textContent = rawContentResult.Value.RawContent.ToString();

                    break;

                default:
                    return new PluginResult(false, false,
                        $"The content type {contentType} is not supported by the {Name} plugin.");
            }

            await _dataPipelineStateService.SaveDataPipelineRunWorkItemArtifacts(
                dataPipelineDefinition,
                dataPipelineRun,
                dataPipelineRunWorkItem,
                [ new DataPipelineStateArtifact
                    {
                        FileName = "content.txt",
                        ContentType = "text/plain",
                        Content = BinaryData.FromString(textContent)
                    },
                    new DataPipelineStateArtifact
                    {
                        FileName = "original-file-name.txt",
                        ContentType = "text/plain",
                        Content = BinaryData.FromString(rawContentResult.Value.Name)
                    }
                ]);

            return
                new PluginResult(true, false);
        }
    }
}
