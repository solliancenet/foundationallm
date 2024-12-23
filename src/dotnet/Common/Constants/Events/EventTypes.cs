namespace FoundationaLLM.Common.Constants.Events
{
    /// <summary>
    /// Provide event type constants.
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        /// The resource provider cache reset command event type.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Cache_ResetCommand = "FoundationaLLM.ResourceProvider.Cache.ResetCommand";

        /// <summary>
        /// The resource provider cache reset command confirmation event type.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Cache_ResetCommandConfirmation = "FoundationaLLM.ResourceProvider.Cache.ResetCommandConfirmation";

        /// <summary>
        /// All event types.
        /// </summary>
        public static List<string> All =>
            [
                FoundationaLLM_ResourceProvider_Cache_ResetCommand,
                FoundationaLLM_ResourceProvider_Cache_ResetCommandConfirmation
            ];
    }
}
