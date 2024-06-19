using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Tasks
{
    /// <summary>
    /// Represents a pool of active tasks with a predefined capacity.
    /// </summary>
    public class TaskPool
    {
        private readonly int _maxConcurrentTasks;
        private readonly List<TaskStatus> _runningStates = new List<TaskStatus>
        {
            TaskStatus.Created,
            TaskStatus.Running,
            TaskStatus.WaitingForActivation,
            TaskStatus.WaitingToRun,
            TaskStatus.WaitingForChildrenToComplete
        };

        private TaskInfo[] _taskInfo;
        private readonly ILogger _logger;

        /// <summary>
        /// Indicates whether the task pool is at capacity or not.
        /// </summary>
        public int AvailableCapacity => _maxConcurrentTasks - _taskInfo.Count(ti => (ti != null) && _runningStates.Contains(ti.Task.Status));

        /// <summary>
        /// Constructs a task pool with a specified capacity.
        /// </summary>
        /// <param name="maxConcurrentTasks">Indicates the maximum number of tasks accepted by the task pool.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public TaskPool(int maxConcurrentTasks,
            ILogger<TaskPool> logger)
        {
            _maxConcurrentTasks = maxConcurrentTasks;
            _taskInfo = new TaskInfo[maxConcurrentTasks];
            _logger = logger;
        }

        /// <summary>
        /// Adds a new batch of tasks to the task pool.
        /// </summary>
        /// <param name="tasks">The list of <see cref="TaskInfo"/> items to be added to the pool.</param>
        /// <exception cref="TaskPoolException">The exception raised when a task cannot be added to the pool (e.g., the task pool is at capacity).</exception>
        public void Add(IEnumerable<TaskInfo> tasks)
        {
            foreach (var t in tasks)
            {
                var indexOfFirstEmptySlot = _taskInfo.TakeWhile(ti => (ti != null) && _runningStates.Contains(ti.Task.Status)).Count();

                if (indexOfFirstEmptySlot == _taskInfo.Length)
                    throw new TaskPoolException("The task pool is at capacity and cannot accept additional tasks");

                _taskInfo[indexOfFirstEmptySlot] = t;
            }
        }

        /// <summary>
        /// Determines whether the task pool already has a running task for a specified payload id.
        /// </summary>
        /// <param name="payloadId">The identifier of the payload.</param>
        /// <returns>True if the task pool already has a running task for the specified payload, false otherwise.</returns>
        public bool HasRunningTaskForPayload(string payloadId)
        {
            var result = _taskInfo.Any(ti => ti != null && ti.PayloadId == payloadId && _runningStates.Contains(ti.Task.Status));
            return result;
        }
            
    }
}
