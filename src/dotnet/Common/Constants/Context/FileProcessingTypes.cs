using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.Context
{
    /// <summary>
    /// Provides constants for the types of file processing.
    /// </summary>
    public static class FileProcessingTypes
    {
        /// <summary>
        /// The file requires no processing.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// The file must be processed by a data pipeline in the context of a conversation.
        /// </summary>
        public const string ConversationDataPipeline = "conversation_data_pipeline";

        /// <summary>
        /// All file processing types.
        /// </summary>
        public static readonly ImmutableArray<string> All = [
            None,
            ConversationDataPipeline
        ];
    }
}
