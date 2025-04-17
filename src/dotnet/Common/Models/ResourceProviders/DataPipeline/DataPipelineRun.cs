using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provider the model for a data pipeline run.
    /// </summary>
    public class DataPipelineRun : AzureCosmosDBResource, IRunnableResource
    {
        /// <summary>
        /// The unique identifier of the data pipeline run.
        /// </summary>
        public string RunId => Id;

        /// <summary>
        /// Gets or sets the object identifier of the data pipeline.
        /// </summary>
        public required string DataPipelineObjectId { get; set; }

        /// <summary>
        /// Gets or sets the name of the manual trigger used to start the pipeline.
        /// </summary>
        public required string TriggerName { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        public required Dictionary<string, object> TriggerParameterValues { get; set; } = [];

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the user that triggered the creation of the data pipeline run.
        /// </summary>
        public required string TriggeringUPN { get; set; }

        /// <inheritdoc/>
        public bool Completed { get; set; }

        /// <inheritdoc/>
        public bool Successful { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public DataPipelineRun() =>
            Type = DataPipelineTypes.DataPipelineRun;

        /// <summary>
        /// Creates a new <see cref="DataPipelineRun"/> instance.
        /// </summary>
        /// <param name="dataPipelineObjectId">The object identifier of the data pipeline.</param>
        /// <param name="triggerName">The name of the data pipeline trigger.</param>
        /// <param name="triggerParameterValues">The dictionary of data pipeline parameter values required by the trigger.</param>
        /// <param name="upn">The UPN that is associated with the data pipeline run.</param>
        /// <param name="triggeringUPN">The UPN that triggered the data pipeline run.</param>
        /// <returns></returns>
        public static DataPipelineRun Create(
            string dataPipelineObjectId,
            string triggerName,
            Dictionary<string, object> triggerParameterValues,
            string upn,
            string triggeringUPN) =>
            new()
            {
                DataPipelineObjectId = dataPipelineObjectId,
                TriggerName = triggerName,
                TriggerParameterValues = triggerParameterValues,
                UPN = upn,

                Name = string.Empty,
                Id = string.Empty,
                InstanceId = string.Empty,
                TriggeringUPN = string.Empty
            };
    }
}
