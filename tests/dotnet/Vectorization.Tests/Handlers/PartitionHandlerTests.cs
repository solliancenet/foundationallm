using FakeItEasy;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Handlers
{
    internal class Partition : ITextSplitterService
    {
        public List<TextChunk> SplitPlainText(string text)
        {
            var position = 1;
            return text.Split("\n").Select(t => new TextChunk
            {
                Position = position++,
                Content = t.Trim(),
            }).ToList();
        }
    }

    internal class PartitionMockServiceFactory : IVectorizationServiceFactory<ITextSplitterService>
    {
        public ITextSplitterService GetService(string serviceName)
        {
            return new Partition();
        }

        public (ITextSplitterService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            throw new NotImplementedException();
        }
    }

    public class PartitionHandlerTests
    {
        [Fact]
        public async void TestProcessRequest()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().Build();
            IConfigurationSection stepsConfiguration = configurationRoot.GetSection("");

            // DI container configuration
            IVectorizationStateService stateService = A.Fake<IVectorizationStateService>();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IVectorizationServiceFactory<ITextSplitterService>, PartitionMockServiceFactory>();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            PartitionHandler handler = new PartitionHandler(
                "Instance123",
                "Queue-Message-1",
                new Dictionary<string, string> { { "text_partition_profile_name", "" } },
                stepsConfiguration,
                stateService,
                serviceCollection.BuildServiceProvider(),
                loggerFactory
            );

            ContentIdentifier contentIdentifier = new ContentIdentifier
            {
                MultipartId = new List<string> {
                    "https://somesa.blob.core.windows.net",
                    "vectorization-input",
                    "somedata.pdf"
                },
                DataSourceObjectId = "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.DataSource/dataSources/datalake01",
                CanonicalId = "SomeBusinessUnit/SomePDFData"
            };
            VectorizationRequest request = new VectorizationRequest
            {
                Name = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                ProcessingType = VectorizationProcessingType.Synchronous,
                Steps = new List<VectorizationStep>
                {
                    new VectorizationStep { Id = "partition", Parameters = new Dictionary<string, string> { } }
                },
                CompletedSteps = new List<string> { },
                RemainingSteps = new List<string> { "partition" }
            };
            List<VectorizationArtifact> vectorizationArtifacts = new List<VectorizationArtifact> {
                // Do not partition: Just whitespace
                new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText, Position = 1, Content = "" },
                // Do not partition: Position = 2
                new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText, Position = 2, Content = "This is an extracted document.\nWith newlines." }
            };
            VectorizationState state = new VectorizationState
            {
                CurrentRequestId = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier,
                Artifacts = vectorizationArtifacts
            };
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            // Nothing to partition
            Assert.False(await handler.Invoke(request, state, tokenSource.Token));

            // Content to partition
            vectorizationArtifacts.Add(new VectorizationArtifact { Type = VectorizationArtifactType.ExtractedText, Position = 1, Content = "This is an extracted document.\nWith newlines." });
            await handler.Invoke(request, state, tokenSource.Token);
            Assert.True(state.Artifacts.Count(artifact => artifact.Type == VectorizationArtifactType.TextPartition) == 2);
        }
    }
}