using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Interface for the Data Pipeline Service.
    /// </summary>
    public interface IDataPipelineService
    {
        /// <summary>
        /// Retrieves a data pipeline run by its name.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="runId">The identifier of the data pipeline run.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The data pipeline run identified by the provided identifier.</returns>
        Task<DataPipelineRun> GetDataPipelineRun(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentiy);

        /// <summary>
        /// Creates a new data pipeline run.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="dataPipelineRun">The object with the properties of the new data pipeline run.</param>
        /// <param name="userIdentiy">The identity of the user running the operation.</param>
        /// <returns>The newly created data pipeline run.</returns>
        Task<DataPipelineRun> CreateDataPipelineRun(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity);
    }
}
