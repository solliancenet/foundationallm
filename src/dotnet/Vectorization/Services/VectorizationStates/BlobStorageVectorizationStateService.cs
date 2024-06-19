using Azure.Core;
using Azure.Storage.Blobs;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parquet.Serialization;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides vectorization state persistence services using  Azure blob storage.
    /// </summary>
    /// <remarks>
    /// Creates a new vectorization state service instance.
    /// </remarks>
    /// <param name="storageService">The <see cref="IStorageService"/> that provides storage services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class BlobStorageVectorizationStateService(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_BlobStorageVectorizationStateService)] IStorageService storageService,
        ILoggerFactory loggerFactory) : VectorizationStateServiceBase, IVectorizationStateService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private const string BLOB_STORAGE_CONTAINER_NAME = "vectorization-state";
        private const string EXECUTION_STATE_DIRECTORY = "execution-state";
        private const string PIPELINE_STATE_DIRECTORY = "pipeline-state";

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request) =>
            await _storageService.FileExistsAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{EXECUTION_STATE_DIRECTORY}/{GetPersistenceIdentifier(request)}.json",
                default);


        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            var content = await _storageService.ReadFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{EXECUTION_STATE_DIRECTORY}/{GetPersistenceIdentifier(request)}.json",
                default);

            return JsonSerializer.Deserialize<VectorizationState>(content)!;
        }

        /// <inheritdoc/>
        public async Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType)
        {
            if (state.LoadedArtifactTypes.Contains(artifactType))
                // This artifact type has already been loaded.
                return;

            var persistenceIdentifier = GetPersistenceIdentifier(state);

            switch (artifactType)
            {
                case VectorizationArtifactType.ExtractedText:

                    var extractedTextArtifact = state.Artifacts.SingleOrDefault(a => a.Type == VectorizationArtifactType.ExtractedText);
                    if (extractedTextArtifact != null)
                        extractedTextArtifact.Content = Encoding.UTF8.GetString(
                            await _storageService.ReadFileAsync(
                            BLOB_STORAGE_CONTAINER_NAME,
                                $"{EXECUTION_STATE_DIRECTORY}/{extractedTextArtifact.CanonicalId}.txt",
                                default));

                    state.LoadedArtifactTypes.Add(VectorizationArtifactType.ExtractedText);
                    break;

                case VectorizationArtifactType.TextPartition:
                case VectorizationArtifactType.TextEmbeddingVector:

                    await AlignItems(state, persistenceIdentifier);

                    state.LoadedArtifactTypes.AddRange(
                        [
                            VectorizationArtifactType.TextPartition,
                            VectorizationArtifactType.TextEmbeddingVector
                        ]);
                    break;

                default:
                    throw new VectorizationException($"The vectorization artifact type {artifactType} is not supported.");
            }
        }

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            var persistenceIdentifier = GetPersistenceIdentifier(state);

            // ExtractedText is persisted as a text file
            var extractedTextArtifact = state.Artifacts.SingleOrDefault(a => a.Type == VectorizationArtifactType.ExtractedText);
            if (extractedTextArtifact != null && extractedTextArtifact.IsDirty)
            {
                extractedTextArtifact.CanonicalId =
                        $"{persistenceIdentifier}_{extractedTextArtifact.Type.ToString().ToLower()}";

                await _storageService.WriteFileAsync(
                    BLOB_STORAGE_CONTAINER_NAME,
                    $"{EXECUTION_STATE_DIRECTORY}/{extractedTextArtifact.CanonicalId}.txt",
                    extractedTextArtifact.Content!,
                    default,
                    default);
            }

            // TextPartition and TextEmbeddingVector are stored together in a Parquet file.

            // Recalculate relevant properties for dirty TextPartition and TextEmbeddingVector artifacts.
            foreach (var artifact in state.Artifacts.Where(a =>
                a.IsDirty
                && (a.Type == VectorizationArtifactType.TextPartition || a.Type == VectorizationArtifactType.TextEmbeddingVector)))
            {
                artifact.ContentHash = string.IsNullOrWhiteSpace(artifact.Content)
                    ? null
                    : HashText(artifact.Content);
                artifact.CanonicalId =
                        $"{persistenceIdentifier}_{artifact.Type.ToString().ToLower()}_{artifact.Position:D6}";
            }

            // Persist TextPartition and TextEmbeddingVector artifacts in Parquet file.
            await SaveTextPartitionsAndEmbeddings(state, persistenceIdentifier);

            var content = JsonSerializer.Serialize(state);
            await _storageService.WriteFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{EXECUTION_STATE_DIRECTORY}/{persistenceIdentifier}.json",
                content,
                default,
                default);
        }

        private async Task SaveTextPartitionsAndEmbeddings(VectorizationState state, string persistenceIdentifier)
        {
            if (!state.Artifacts.Any(a =>
                a.IsDirty
                && (a.Type == VectorizationArtifactType.TextPartition || a.Type == VectorizationArtifactType.TextEmbeddingVector)))
                // No relevant artifacts have changed so there is nothing to update in the Parquet file
                return;

            // Align in-memory artifacts with persisted ones
            await AlignItems(state, persistenceIdentifier);

            // Only consider non-empty text partitions.
            var textPartitions = state.Artifacts
                .Where(a =>
                    a.Type == VectorizationArtifactType.TextPartition
                    && !string.IsNullOrEmpty(a.Content))
                .OrderBy(a => a.Position)
                .ToList();
            if (textPartitions.Count == 0)
                // If there are no text partitions then there is nothing to save.
                return;

            var serializerOptions = new JsonSerializerOptions { Converters = { new Embedding.JsonConverter() } };

            // Align text embeddings with partition and set defaults for embeddings that are not present.
            var items =
                from tp in textPartitions
                from te in state.Artifacts
                    .Where(a =>
                        a.Type == VectorizationArtifactType.TextEmbeddingVector
                        && a.Position == tp.Position)
                    .DefaultIfEmpty()
                select new VectorizationStateItem
                {
                    PipelineName = state.PipelineName ?? "NoPipeline",
                    Position = tp.Position,
                    TextPartitionContent = tp.Content!,
                    TextPartitionHash = tp.ContentHash,
                    TextPartitionSize = tp.Size,
                    TextEmbeddingVectorSize = te?.Size ?? 0,
                    TextEmbeddingVector = (te == null || string.IsNullOrWhiteSpace(te.Content))
                        ? null
                        : JsonSerializer.Deserialize<Embedding>(te.Content, serializerOptions).Vector.ToArray().ToList(),
                    TextEmbeddingVectorHash = te?.ContentHash
                };

            await SaveItems(items.ToList(), persistenceIdentifier);
        }

        private async Task SaveItems(List<VectorizationStateItem> items, string persistenceIdentifier)
        {
            var serializedParquet = new MemoryStream();
            await ParquetSerializer.SerializeAsync(
                items,
                serializedParquet,
                new ParquetSerializerOptions() { CompressionMethod = Parquet.CompressionMethod.Snappy });

            await _storageService.WriteFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{EXECUTION_STATE_DIRECTORY}/{persistenceIdentifier}.snappy.parquet",
                serializedParquet,
                "application/vnd.apache.parquet",
                default);
        }

        private async Task<Dictionary<int, VectorizationStateItem>> LoadItems(string persistenceIdentifier)
        {
            var filePath = $"{EXECUTION_STATE_DIRECTORY}/{persistenceIdentifier}.snappy.parquet";

            if (!await _storageService.FileExistsAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                filePath,
                default))
                return [];

            // Load data from the associated Parquet file
            var binaryContent = await _storageService.ReadFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                filePath,
                default);
            return
                (await ParquetSerializer
                        .DeserializeAsync<VectorizationStateItem>(binaryContent.ToStream()))
                        .ToDictionary(x => x.Position, x => x);
        }

        private async Task AlignItems(VectorizationState state,  string persistenceIdentifier)
        {
            // Load data from the associated Parquet file
            var items = await LoadItems(persistenceIdentifier);

            // Update TextPartition artifacts
            foreach (var textPartitionArtifact in state.Artifacts
                .Where(a => a.Type == VectorizationArtifactType.TextPartition && !a.IsDirty))
                if (items.TryGetValue(textPartitionArtifact.Position, out var item))
                {
                    textPartitionArtifact.Content = item.TextPartitionContent;
                    textPartitionArtifact.ContentHash = item.TextPartitionHash;
                    textPartitionArtifact.Size = item.TextPartitionSize;
                }

            var serializerOptions = new JsonSerializerOptions { Converters = { new Embedding.JsonConverter() } };

            foreach (var textEmbeddingArtifact in state.Artifacts
                .Where(a => a.Type == VectorizationArtifactType.TextEmbeddingVector && !a.IsDirty))
                if (items.TryGetValue(textEmbeddingArtifact.Position, out var item))
                {
                    textEmbeddingArtifact.Content = item.TextEmbeddingVector == null
                        ? null
                        : JsonSerializer.Serialize(
                            new Embedding([.. item.TextEmbeddingVector]),
                            serializerOptions);
                    textEmbeddingArtifact.ContentHash = item.TextEmbeddingVectorHash;
                    textEmbeddingArtifact.Size = item.TextEmbeddingVectorSize;
                }
        }

        /// <inheritdoc/>
        public async Task SavePipelineState(VectorizationPipelineState state)
        {
            //pipeline object id format: "/instances/{instanceId}/providers/FoundationaLLM.Vectorization/vectorizationPipelines/{pipeline-name}"
            var pipelineName = state.PipelineObjectId.Split('/').Last();
            //vectorization-state/pipeline-state/pipeline-name/pipeline-name-pipeline-execution-id.json
            var pipelineStatePath = $"{PIPELINE_STATE_DIRECTORY}/{pipelineName}/{pipelineName}-{state.ExecutionId}.json";
            var content = JsonSerializer.Serialize(state);

            // add SemaphoreSlim async lock to avoid pipeline file contention - allows for a lock with an await in the body
            await _semaphore.WaitAsync();
            try
            {
                await _storageService.WriteFileAsync(
                    BLOB_STORAGE_CONTAINER_NAME,
                    pipelineStatePath,
                    content,
                    default,
                    default);
            }
            finally
            {
                _semaphore.Release();
            }           
        }

        /// <inheritdoc/>
        public async Task<VectorizationPipelineState> ReadPipelineState(string pipelineName, string pipelineExecutionId)
        {
            var pipelineStatePath = $"{PIPELINE_STATE_DIRECTORY}/{pipelineName}/{pipelineName}-{pipelineExecutionId}.json";
            var content = await _storageService.ReadFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                pipelineStatePath,
                default);

            return JsonSerializer.Deserialize<VectorizationPipelineState>(content)!;

        }
    }
}
