using Azure.Core;
using Azure.Storage.Blobs;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request) =>
            await _storageService.FileExistsAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{GetPersistenceIdentifier(request.ContentIdentifier)}.json",
                default);


        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            var content = await _storageService.ReadFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{GetPersistenceIdentifier(request.ContentIdentifier)}.json",
                default);

            return JsonSerializer.Deserialize<VectorizationState>(content)!;
        }

        /// <inheritdoc/>
        public async Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType)
        {
            foreach (var artifact in state.Artifacts.Where(a => a.Type == artifactType))
                if (!string.IsNullOrWhiteSpace(artifact.CanonicalId))
                    artifact.Content = Encoding.UTF8.GetString(
                        await _storageService.ReadFileAsync(
                            BLOB_STORAGE_CONTAINER_NAME,
                            artifact.CanonicalId,
                            default));
        }

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            var persistenceIdentifier = GetPersistenceIdentifier(state.ContentIdentifier);

            foreach (var artifact in state.Artifacts)
                if (artifact.IsDirty)
                {
                    var artifactPath =
                        $"{persistenceIdentifier}_{artifact.Type.ToString().ToLower()}_{artifact.Position:D6}.txt";

                    await _storageService.WriteFileAsync(
                        BLOB_STORAGE_CONTAINER_NAME,
                        artifactPath,
                        artifact.Content!,
                        default,
                        default);
                    artifact.CanonicalId = artifactPath;
                }

            var content = JsonSerializer.Serialize(state);
            await _storageService.WriteFileAsync(
                BLOB_STORAGE_CONTAINER_NAME,
                $"{persistenceIdentifier}.json",
                content,
                default,
                default);
        }
    }
}
