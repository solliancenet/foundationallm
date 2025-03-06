namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Types of data pipeline triggers.
    /// </summary>
    public enum DataPipelineTriggerType
    {
        /// <summary>
        /// The data pipeline is triggered manually.
        /// </summary>
        Manual,

        /// <summary>
        /// The data pipeline is triggered based on a regular schedule.
        /// </summary>
        Schedule,

        /// <summary>
        /// The data pipeline is triggered based on content change events (e.g., an existing file is updated or a new file is added).
        /// </summary>
        Event
    }
}
