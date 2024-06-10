namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Types of vectori indexes available to store embeddings.
    /// </summary>
    public enum IndexerType
    {
        /// <summary>
        /// Indexer using Azure AI Search vector indexes.
        /// </summary>
        AzureAISearchIndexer,

        /// <summary>
        /// Indexer using Azure Cosmos DB NoSQL indexes.
        /// </summary>
        AzureCosmosDBNoSQLIndexer,

        /// <summary>
        /// Indexer using PostgreSQL indexes.
        /// </summary>
        PostgresIndexer
    }
}
