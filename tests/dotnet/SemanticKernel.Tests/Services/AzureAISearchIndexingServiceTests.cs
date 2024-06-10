using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services.Indexing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemanticKernel.Tests.Models;

namespace FoundationaLLM.SemanticKernel.Tests.Services
{
    public class AzureAiSearchIndexingServiceTests
    {
        private readonly SearchIndexClient _searchIndexClient;
        private readonly IIndexingService _indexingService;
        private readonly string _indexName = Environment.GetEnvironmentVariable("AzureAISearchIndexingServiceTestsCollectionName") ?? "semantickernel-integration-tests";

        public AzureAiSearchIndexingServiceTests()
        {
            var endpoint = Environment.GetEnvironmentVariable("AzureAISearchIndexingServiceTestsSearchEndpoint") ?? "";
            _searchIndexClient = new SearchIndexClient(
                new Uri(endpoint),
                DefaultAuthentication.AzureCredential
            );
            _indexingService = new AzureAISearchIndexingService(
                Options.Create(
                    new AzureAISearchIndexingServiceSettings
                    {
                        Endpoint = endpoint,
                        AuthenticationType = AzureAISearchAuthenticationTypes.AzureIdentity
                    }
                ),
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AzureAISearchIndexingService>()
            );
        }

        private async Task CreateIndex()
        {
            var searchIndex = new SearchIndex(
                _indexName,
                new FieldBuilder().Build(typeof(TestIndexSchema))
            )
            {
                VectorSearch = new VectorSearch()
            };

            searchIndex.VectorSearch.Algorithms.Add(
                new HnswAlgorithmConfiguration("algorithm-config")
            );

            searchIndex.VectorSearch.Profiles.Add(
                new VectorSearchProfile("vector-config", "algorithm-config")
            );

            await _searchIndexClient.CreateIndexAsync(
                searchIndex
            );
        }

        [Fact]
        public async void TestIndexEmbeddingsAsync()
        {
            await CreateIndex();

            EmbeddedContent embeddedContent = new EmbeddedContent
            {
                ContentId = new ContentIdentifier
                {
                    MultipartId = new List<string> {
                        "https://somesa.blob.core.windows.net",
                        "vectorization-input",
                        "somedata.pdf"
                    },
                    DataSourceObjectId = "SomePDFData",
                    CanonicalId = "SomeBusinessUnit/SomePDFData"
                },
                ContentParts = new List<EmbeddedContentPart>
                {
                    new EmbeddedContentPart {
                        Id = "1",
                        Content = "This is Phrase #1",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    },
                    new EmbeddedContentPart {
                        Id = "2",
                        Content = "This is Phrase #2",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    },
                    new EmbeddedContentPart {
                        Id = "3",
                        Content = "This is Phrase #3",
                        Embedding = new Embedding
                        {
                            Vector = new ReadOnlyMemory<float>(Enumerable.Repeat<float>(0, 1536).ToArray())
                        }
                    }
                }
            };

            Assert.Equal(
                3,
                (await _indexingService.IndexEmbeddingsAsync(embeddedContent, _indexName)).Count
            );

            await _searchIndexClient.DeleteIndexAsync(_indexName);
        }
    }
}