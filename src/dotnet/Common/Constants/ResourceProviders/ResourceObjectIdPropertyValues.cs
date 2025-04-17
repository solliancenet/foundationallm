namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants of the resource property values.
    /// </summary>
    public static class ResourceObjectIdPropertyValues
    {
        /// <summary>
        /// Main model.
        /// </summary>
        public const string MainModel = "main_model";

        /// <summary>
        /// Main prompt.
        /// </summary>
        public const string MainPrompt = "main_prompt";

        /// <summary>
        /// Router prompt.
        /// </summary>
        public const string RouterPrompt = "router_prompt";

        /// <summary>
        /// The data pipeline used to upload files.
        /// </summary>
        public const string FileUploadDataPipeline = "file_upload_data_pipeline";

        /// <summary>
        /// The vector store container used to store individual vector stores.
        /// </summary>
        public const string VectorDatabase = "vector_database";

        /// <summary>
        /// Main indexing profile.
        /// </summary>
        public const string MainIndexingProfile = "main_indexing_profile";

        /// <summary>
        /// Tool association.
        /// </summary>
        public const string ToolAssociation = "tool_association";
    }
}
