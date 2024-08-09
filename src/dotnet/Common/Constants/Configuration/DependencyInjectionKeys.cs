// -------------------------------------------------------------------------------
//
// WARNING!
// This file is auto-generated based on the AppConfiguration.json file.
// Do not make changes to this file, as they will be automatically overwritten.
//
// -------------------------------------------------------------------------------
namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Defines all keys used for named dependency injection.
    /// </summary>
    public static partial class DependencyInjectionKeys
    {        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.AIModel resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_AIModel =
            "FoundationaLLM:ResourceProviders:AIModel";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.Agent resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Agent =
            "FoundationaLLM:ResourceProviders:Agent";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.Attachment resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Attachment =
            "FoundationaLLM:ResourceProviders:Attachment";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.Configuration resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Configuration =
            "FoundationaLLM:ResourceProviders:Configuration";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.DataSource resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_DataSource =
            "FoundationaLLM:ResourceProviders:DataSource";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.Prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Prompt =
            "FoundationaLLM:ResourceProviders:Prompt";
        
        /// <summary>
        /// Dependency injection key used by the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Vectorization =
            "FoundationaLLM:ResourceProviders:Vectorization";
        
        /// <summary>
        /// Dependency injection key used to inject the implementation of IIndexingService based on Azure Cosmos DB NoSQL.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureCosmosDBNoSQLVectorStore_Configuration =
            "FoundationaLLM:APIEndpoints:AzureCosmosDBNoSQLVectorStore:Configuration";
        
        /// <summary>
        /// Dependency injection key used to inject the implementation of IIndexingService based on Azure PostgreSQL.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzurePostgreSQLVectorStore_Configuration =
            "FoundationaLLM:APIEndpoints:AzurePostgreSQLVectorStore:Configuration";
        
        /// <summary>
        /// Dependency injection key used to inject the IConfiguration section for vectorization steps.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Steps =
            "FoundationaLLM:Vectorization:Steps";
        
        /// <summary>
        /// Dependency injection key used to inject the IConfiguration section for vectorization queues.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues =
            "FoundationaLLM:Vectorization:Queues";
        
        /// <summary>
        /// Dependency injection key used to inject storage settings for the Vectorization state service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_StateService_Storage =
            "FoundationaLLM:Vectorization:StateService:Storage";
        
        /// <summary>
        /// Dependency injection key used to inject the ITextEmbeddingService implementation that uses the FoundationaLLM Gateway for embedding.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbedding_Gateway =
            "FoundationaLLM:Vectorization:TextEmbedding:Gateway";
    }
}
