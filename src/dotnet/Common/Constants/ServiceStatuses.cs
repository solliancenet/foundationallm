namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Service status constants.
    /// </summary>
    public static class ServiceStatuses
    {
        /// <summary>
        /// The service is in an initializing state.
        /// </summary>
        public const string Initializing = "Initializing";

        /// <summary>
        /// The service is operational and ready to accept requests.
        /// </summary>
        public const string Ready = "Ready";

        /// <summary>
        /// The service is partially available. This is usually caused by
        /// one or more subordinate services being in a warning or error state.
        /// </summary>
        public const string PartiallyAvailable = "Partially_Available";

        /// <summary>
        /// The service is in a warning state.
        /// </summary>
        public const string Warning = "Warning";

        /// <summary>
        /// The service is in an error state.
        /// </summary>
        public const string Error = "Error";
    }
}
