using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipeline.Interfaces
{
    /// <summary>
    /// Defines the interface for Data Pipeline API clients.
    /// </summary>
    public interface IDataPipelineServiceClient
    {
        /// <summary>
        /// Gets a data pipeline run by its identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="runId">The data pipeline run identifier.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The requested data pipeline run object.</returns>
        Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The data pipeline run to create.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity);
    }
}
