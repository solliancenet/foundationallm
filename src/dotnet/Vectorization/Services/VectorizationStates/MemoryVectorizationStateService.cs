using Azure.Core;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides in-memory vectorization state persistence.
    /// </summary>
    public class MemoryVectorizationStateService : VectorizationStateServiceBase, IVectorizationStateService
    {
        private readonly Dictionary<string, VectorizationState> _vectorizationStateDictionary = [];

        /// <inheritdoc/>
        public async Task<bool> HasState(VectorizationRequest request)
        {
            await Task.CompletedTask;

            return _vectorizationStateDictionary.ContainsKey(
                GetPersistenceIdentifier(request.ContentIdentifier));
        }

        /// <inheritdoc/>
        public async Task<VectorizationState> ReadState(VectorizationRequest request)
        {
            await Task.CompletedTask;
            var id = GetPersistenceIdentifier(request.ContentIdentifier);

            if (!_vectorizationStateDictionary.TryGetValue(id, out VectorizationState? value))
                throw new ArgumentException($"Vectorization state for content id [{id}] could not be found.");

            return value;
        }

        /// <inheritdoc/>
        public async Task LoadArtifacts(VectorizationState state, VectorizationArtifactType artifactType) =>
            await Task.CompletedTask;

        /// <inheritdoc/>
        public async Task SaveState(VectorizationState state)
        {
            await Task.CompletedTask;
            var id = GetPersistenceIdentifier(state.ContentIdentifier);

            ArgumentNullException.ThrowIfNull(state);

            if (!_vectorizationStateDictionary.TryAdd(id, state))
                _vectorizationStateDictionary[id] = state;
        }
    }
}
