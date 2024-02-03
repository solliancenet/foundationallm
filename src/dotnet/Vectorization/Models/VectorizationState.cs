using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Vectorization.Interfaces;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Holds the state associated with each step of the vectorization pipeline
    /// for a specified content item (i.e., document).
    /// </summary>
    public class VectorizationState
    {
        /// <summary>
        /// The unique identifier of the current vectorization request. Subsequent vectorization requests
        /// referring to the same content will have different unique identifiers.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("request_id")]
        public required string CurrentRequestId { get; set; }

        /// <summary>
        /// The <see cref="ContentIdentifier"/> object identifying the content being vectorized.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("content_identifier")]
        public required ContentIdentifier ContentIdentifier { get; set; }

        /// <summary>
        /// The vectorization artifacts associated with the vectorization state.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("artifacts")]
        public List<VectorizationArtifact> Artifacts { get; set; } = [];

        /// <summary>
        /// The vector index references associated with the vectorization state.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("index_references")]
        public List<VectorizationIndexReference> IndexReferences { get; set; } = [];

        /// <summary>
        /// The list of vectorization requests associated with the content identified by <see cref="ContentIdentifier"/>.
        /// </summary>
        [JsonPropertyOrder(19)]
        [JsonPropertyName("requests")]
        public List<VectorizationRequest> Requests { get; set; } = [];

        /// <summary>
        /// The list of log entries associated with actions executed by the vectorization pipeline.
        /// </summary>
        [JsonPropertyOrder(20)]
        [JsonPropertyName("log")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<VectorizationLogEntry> LogEntries { get; set; } = [];

        /// <summary>
        /// Adds a new generic log entry.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
        /// <param name="text">The string content of the log entry.</param>
        public void Log(IVectorizationStepHandler handler, string requestId, string messageId, string text) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, messageId, handler.StepId, text));

        /// <summary>
        /// Adds a log entry marking the start of handling.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
        public void LogHandlerStart(IVectorizationStepHandler handler, string requestId, string messageId) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, messageId, handler.StepId, "Started handling step."));

        /// <summary>
        /// Adds a log entry marking the completion of handling.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
        public void LogHandlerEnd(IVectorizationStepHandler handler, string requestId, string messageId) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, messageId, handler.StepId, "Finished handling step."));

        /// <summary>
        /// Adds a log entry for a handling exception.
        /// </summary>
        /// <param name="handler">The vectorization step handler executing the action.</param>
        /// <param name="requestId">The identifier of the vectorization request.</param>
        /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
        /// <param name="ex">The exception being logged.</param>
        public void LogHandlerError(IVectorizationStepHandler handler, string requestId, string messageId, Exception ex) =>
            LogEntries.Add(new VectorizationLogEntry(
                requestId, messageId, handler.StepId, $"ERROR: {ex.Message}"));

        /// <summary>
        /// Creates a new <see cref="VectorizationState"/> instance based on a specified vectorization request.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> instance for which the state is created.</param>
        /// <returns>The <see cref="VectorizationState"/> created from the request.</returns>
        public static VectorizationState FromRequest(VectorizationRequest request) =>
            new()
            {
                CurrentRequestId = request.Id!,
                ContentIdentifier = request.ContentIdentifier
            };

        /// <summary>
        /// Adds or replaces a vectorization artifact associated with the vectorization state.
        /// </summary>
        /// <param name="artifact">The <see cref="VectorizationArtifact"/> to be added or replaced.</param>
        public void AddOrReplaceArtifact(VectorizationArtifact artifact)
        {
            var existingArtifact = Artifacts.SingleOrDefault(a => a.Type == artifact.Type & a.Position == artifact.Position);
            if (existingArtifact != null)
                Artifacts.Remove(existingArtifact);

            artifact.IsDirty = true;
            Artifacts.Add(artifact);
        }

        /// <summary>
        /// Adds or replaces the list of vector index references associated with the vectorization state.
        /// </summary>
        /// <param name="indexEntryIds">The ordered list of vector index entry identifiers.</param>
        public void AddOrReplaceIndexReferences(IList<string> indexEntryIds) =>
            IndexReferences = Enumerable.Range(0, indexEntryIds.Count)
                .Select(i => new VectorizationIndexReference
                {
                    IndexEntryId = indexEntryIds[i],
                    Position = i
                })
                .ToList();

        /// <summary>
        /// Adds a vectorization request to the list of requests if it is not already there.
        /// </summary>
        /// <param name="request">The <see cref="VectorizationRequest"/> being added.</param>
        public void AddRequestIfMissing(VectorizationRequest request)
        {
            if (Requests.Any(r => r.ObjectId == request.ObjectId))
                return;

            Requests.Add(request);
        }
    }
}
