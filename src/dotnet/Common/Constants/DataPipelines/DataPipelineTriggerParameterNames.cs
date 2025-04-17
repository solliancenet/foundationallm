namespace FoundationaLLM.Common.Constants.DataPipelines
{
    /// <summary>
    /// Constants for well-known data pipeline trigger parameter names.
    /// </summary>
    public static class DataPipelineTriggerParameterNames
    {
        /// <summary>
        /// The context file object identifier for the ContextFile data source.
        /// </summary>
        public const string DataSourceContextFileContextFileObjectId = "DataSource.ContextFile.ContextFileObjectId";

        /// <summary>
        /// The vector store template object identifier for the indexing stage.
        /// </summary>
        public const string StageIndexVectorDatabaseObjectId = "Stage.Index.VectorDatabaseObjectId";

        /// <summary>
        /// The vector store identifier for the indexing stage.
        /// </summary>
        public const string StageIndexVectorStoreId = "Stage.Index.VectorStoreId";
    }
}
