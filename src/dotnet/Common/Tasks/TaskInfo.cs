namespace FoundationaLLM.Common.Tasks
{
    /// <summary>
    /// Provides the context for a task running in the task pool.
    /// </summary>
    public class TaskInfo
    {
        /// <summary>
        /// The unique identifier of the payload processed by the task.
        /// </summary>
        public required string PayloadId { get; set; }

        /// <summary>
        /// The <see cref="Task"/> being run.
        /// </summary>
        public required Task Task { get; set; }


        /// <summary>
        /// The start time of the task.
        /// </summary>
        public required DateTimeOffset StartTime { get; set; }
    }
}
