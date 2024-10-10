using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Orchestration.Core.Interfaces;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Base class for an orchestration involving a FoundationaLLM agent.
    /// </summary>
    /// <remarks>
    /// Constructor for the OrchestrationBase class.
    /// </remarks>
    /// <param name="orchestrationService"></param>
    public class OrchestrationBase(ILLMOrchestrationService orchestrationService)
    {
        /// <summary>
        /// The orchestration service for the agent.
        /// </summary>
        protected readonly ILLMOrchestrationService _orchestrationService = orchestrationService;

        /// <summary>
        /// The call to execute a completion after the agent is configured.
        /// </summary>
        /// <param name="completionRequest"></param>
        /// <returns></returns>
        public virtual async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null!;
        }

        /// <summary>
        /// Starts a completion operation.
        /// </summary>
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> providing details about the completion request.</param>
        /// <returns>A <see cref="LongRunningOperation"/> object providing details about the newly started operation.</returns>
        public virtual async Task<LongRunningOperation> StartCompletionOperation(CompletionRequest completionRequest)
        {
            await Task.CompletedTask;
            return null!;
        }

        /// <summary>
        /// Gets the status of a completion operation.
        /// </summary>
        /// <param name="operationId">The identifier of the completion operation.</param>
        /// <returns>A <see cref="LongRunningOperation"/> object providing details about the running operation.</returns>
        public virtual async Task<LongRunningOperation> GetCompletionOperationStatus(string operationId)
        {
            await Task.CompletedTask;
            return null!;
        }
    }
}
