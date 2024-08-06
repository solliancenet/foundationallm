using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SemanticKernel.Tests.Services
{
    public class SemanticKernelTextEmbeddingServiceTests
    {
        private SemanticKernelTextEmbeddingService _semanticKernelTextEmbeddingService;
        
        public SemanticKernelTextEmbeddingServiceTests()
        {
            _semanticKernelTextEmbeddingService = new SemanticKernelTextEmbeddingService(
                Options.Create(
                    new SemanticKernelTextEmbeddingServiceSettings { 
                        AuthenticationType = AuthenticationTypes.AzureIdentity,
                        DeploymentName = "embeddings",
                        Endpoint = Environment.GetEnvironmentVariable("SemanticKernelTextEmbeddingServiceTestsOpenAiEndpoint") ?? ""
                    }
                ),
                LoggerFactory.Create(builder => builder.AddConsole())
            );
        }

        [Fact]
        public async void TestGetEmbedding()
        {
            var embeddingResult = await _semanticKernelTextEmbeddingService.GetEmbeddingsAsync(
                "Instance123",
                [new TextChunk { Position = 1, Content = "Some Test Text" }]);
            Assert.True(embeddingResult.TextChunks.Count > 0);
            Assert.IsType<int>(embeddingResult.TokenCount);
        }

        [Fact]
        public async void TestGetEmbeddings()
        {
            var embeddingResult = await _semanticKernelTextEmbeddingService.GetEmbeddingsAsync(
                "Instance123",
                [new TextChunk { Position = 1, Content = "Some Test Text" }]);
            Assert.True(embeddingResult.TextChunks.Count > 0);
            Assert.IsType<int>(embeddingResult.TokenCount);
        }
    }
}
