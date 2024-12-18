namespace FoundationaLLM.Common.Constants.Events
{
    /// <summary>
    /// Provide event type constants.
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        /// The event type for FoundationaLLM.Agent events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Agent = "ResourceProvider.FoundationaLLM.Agent";

        /// <summary>
        /// The event type for FoundationaLLM.Vectorization events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Vectorization = "ResourceProvider.FoundationaLLM.Vectorization";

        /// <summary>
        /// The event type for FoundationaLLM.Configuration events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Configuration = "ResourceProvider.FoundationaLLM.Configuration";

        /// <summary>
        /// The event type for FoundationaLLM.DataSource events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_DataSource = "ResourceProvider.FoundationaLLM.DataSource";

        /// <summary>
        /// The event type for FoundationaLLM.Attachment events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Attachment = "ResourceProvider.FoundationaLLM.Attachment";

        /// <summary>
        /// The event type for FoundationaLLM.AIModel events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_AIModel = "ResourceProvider.FoundationaLLM.AIModel";

        /// <summary>
        /// The event type for FoundationaLLM.AzureOpenAI events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_AzureOpenAI = "ResourceProvider.FoundationaLLM.AzureOpenAI";

        /// <summary>
        /// The event type for FoundationaLLM.Conversation events.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Conversation = "ResourceProvider.FoundationaLLM.Conversation";

        /// <summary>
        /// All event types.
        /// </summary>
        public static List<string> All =>
            [
                FoundationaLLM_ResourceProvider_Agent,
                FoundationaLLM_ResourceProvider_Vectorization,
                FoundationaLLM_ResourceProvider_Configuration,
                FoundationaLLM_ResourceProvider_DataSource,
                FoundationaLLM_ResourceProvider_Attachment,
                FoundationaLLM_ResourceProvider_AIModel,
                FoundationaLLM_ResourceProvider_AzureOpenAI,
                FoundationaLLM_ResourceProvider_Conversation
            ];
    }
}
