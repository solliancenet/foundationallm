namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides the settings for knowledge search.
    /// </summary>
    public class KnowledgeSearchSettings
    {
        /// <summary>
        /// The object identifier of the data pipeline used to process uploaded files.
        /// </summary>
        public required string FileUploadDataPipelineObjectId { get; set; }

        /// <summary>
        /// The object identifier of the vector database used to store the processed uploaded files.
        /// </summary>
        public required string FileUploadVectorDatabaseObjectId { get; set; }
    }
}
