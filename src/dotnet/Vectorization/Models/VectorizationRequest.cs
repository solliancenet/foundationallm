using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Vectorization.Exceptions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization request.
    /// </summary>
    public class VectorizationRequest
    {
        /// <summary>
        /// The unique identifier of the vectorization request.
        /// The responsibility to create this identifier belongs to the initiator of the vectorization request.
        /// </summary>
        [JsonPropertyOrder(-1)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// The unique identifier of the vectorization request.
        /// The responsibility to create this identifier belongs to the FoundationaLLM.Vectorization resource manager.
        /// Subsequent vectorization requests referring to the same content will have different unique identifiers.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("object_id")]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The <see cref="ContentIdentifier"/> object identifying the content being vectorized.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("content_identifier")]
        public required ContentIdentifier ContentIdentifier { get; set; }

        /// <summary>
        /// The <see cref="VectorizationProcessingType"/> indicating how should the request be processed.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("processing_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required VectorizationProcessingType ProcessingType { get; set; }

        /// <summary>
        /// The list of vectorization steps requested by the vectorization request.
        /// Vectorization steps are identified by unique names like "extract", "partition", "embed", "index", etc.
        /// </summary>
        [JsonPropertyOrder(10)]
        [JsonPropertyName("steps")]
        public required List<VectorizationStep> Steps { get; set; }

        /// <summary>
        /// The ordered list of the names of the vectorization steps that were already completed.
        /// </summary>
        [JsonPropertyOrder(11)]
        [JsonPropertyName("completed_steps")]
        public List<string> CompletedSteps { get; set; } = [];

        /// <summary>
        /// The ordered list of the names of the vectorization steps that still need to be executed.
        /// </summary>
        [JsonPropertyOrder(12)]
        [JsonPropertyName("remaining_steps")]
        public List<string> RemainingSteps { get; set; } = [];

        /// <summary>
        /// Indicates whether the vectorization process is complete or not.
        /// </summary>
        [JsonIgnore]
        public bool Complete => RemainingSteps.Count == 0;

        /// <summary>
        /// The current step of the vectorization request.
        /// </summary>
        [JsonIgnore]
        public string? CurrentStep => RemainingSteps.Count == 0
            ? null
            : RemainingSteps.First();

        /// <summary>
        /// Advances the vectorization pipeline to the next step.
        /// The newly set current step is used to choose the next request source to which the vectorization request will be added.
        /// </summary>
        /// <returns>A tuple containing the name of the previous step and the name of the next step to execute if there are steps left to execute or null if the processing
        /// of the vectorization request is complete.</returns>
        public (string PreviousStep, string? CurrentStep) MoveToNextStep()
        {
            if (RemainingSteps.Count == 0)
                throw new VectorizationException($"Attempting to move to the next step when no steps remain for vectorization request with id {Id}.");

            var previousStepName = RemainingSteps.First();
            RemainingSteps.RemoveAt(0);
            CompletedSteps.Add(previousStepName);

            var nextStepName = RemainingSteps.Count == 0
                ? null
                : RemainingSteps[0];

            return (previousStepName, nextStepName);
        }

        /// <summary>
        /// Reverts the vectorization pipeline to the previous step, returning the name of the step to execute
        /// </summary>
        public string RollbackToPreviousStep()
        {
            if (CompletedSteps.Count == 0)
            {
                throw new VectorizationException("The list of completed steps is empty");
            }

            var stepName = CompletedSteps.Last();
            CompletedSteps.RemoveAt(this.CompletedSteps.Count - 1);
            RemainingSteps.Insert(0, stepName);

            return stepName;
        }

        /// <summary>
        /// Gets the vectorization pipeline step that has a specific identifier.
        /// </summary>
        /// <param name="step">The identifier of the vectorization pipeline step.</param>
        /// <returns>An instances of the <see cref="VectorizationStep"/> class with the details required by the step handler.</returns>
        public VectorizationStep? this[string step] =>
            Steps.SingleOrDefault(s => s.Id == step);
    }
}
