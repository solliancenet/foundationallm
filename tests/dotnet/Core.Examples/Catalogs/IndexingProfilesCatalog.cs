using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class IndexingProfilesCatalog
    {
        public static readonly List<IndexingProfile> Items =
        [
            new IndexingProfile { Name = "indexing_profile_really_big", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "reallybig" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" }}, IndexingAPIEndpointConfigurationObjectId=TestAPIEndpointConfigurationNames.DefaultAzureAISearch},
            new IndexingProfile { Name = "indexing_profile_pdf", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "pdf" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, IndexingAPIEndpointConfigurationObjectId=TestAPIEndpointConfigurationNames.DefaultAzureAISearch},
            new IndexingProfile { Name = "indexing_profile_sdzwa", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string>{ { "IndexName", "fllm-pdf" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, IndexingAPIEndpointConfigurationObjectId=TestAPIEndpointConfigurationNames.DefaultAzureAISearch},
            new IndexingProfile { Name = "indexing_profile_dune", Indexer = IndexerType.AzureAISearchIndexer, Settings = new Dictionary<string, string> { { "IndexName", "fllm-dune" }, { "TopN", "3" }, { "Filters", "" }, { "EmbeddingFieldName", "Embedding" }, { "TextFieldName", "Text" } }, IndexingAPIEndpointConfigurationObjectId=TestAPIEndpointConfigurationNames.DefaultAzureAISearch}
        ];

        public static List<IndexingProfile> GetIndexingProfiles()
        {
            var items = new List<IndexingProfile>();
            items.AddRange(Items);
            return items;
        }
    }
}
