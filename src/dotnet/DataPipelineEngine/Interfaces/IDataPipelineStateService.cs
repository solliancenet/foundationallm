using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for the Data Pipeline State Service.
    /// </summary>
    public interface IDataPipelineStateService
    {
        /// <summary>
        /// Gets a data pipeline run by its identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun?> GetDataPipelineRun(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentity);
    }
}
