using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class IndexingProfilesCatalog
    {
        public static readonly List<IndexingProfile> Items =
        [
            new IndexingProfile { Name = "indexing_profile_different_file_types", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "fllm-org-data" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, ConfigurationReferences = new Dictionary<string, string>{ { "AuthenticationType", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType" }, { "Endpoint", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint" } } },
            new IndexingProfile { Name = "indexing_profile_really_big", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "reallybig" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, ConfigurationReferences = new Dictionary<string, string>{ { "AuthenticationType", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType" }, { "Endpoint", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint" } } },
            new IndexingProfile { Name = "indexing_profile_pdf", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "pdf" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, ConfigurationReferences = new Dictionary<string, string>{ { "AuthenticationType", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType" }, { "Endpoint", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint" } } },
            new IndexingProfile { Name = "indexing_profile_sdzwa", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "fllm-pdf" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, ConfigurationReferences = new Dictionary<string, string>{ { "AuthenticationType", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType" }, { "Endpoint", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint" } } },
            new IndexingProfile { Name = "indexing_profile_dune", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string> { { "IndexName", "fllm-dune" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, ConfigurationReferences = new Dictionary<string, string> { { "AuthenticationType", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType" }, { "Endpoint", "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint" } } }
        ];

        public static List<IndexingProfile> GetIndexingProfiles()
        {
            var items = new List<IndexingProfile>();
            items.AddRange(Items);
            return items;
        }
    }
}
