using FakeItEasy;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Services.ContentSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Handlers
{
    internal class MockContentSourceService : ContentSourceServiceBase, IContentSourceService
    {
        public Task<string> ExtractTextAsync(ContentIdentifier contentId, CancellationToken cancellationToken)
        {
            return Task.FromResult("This is the PDF document data.");
        }
    }

    internal class MockServiceFactory : IVectorizationServiceFactory<IContentSourceService>
    {
        public IContentSourceService GetService(string serviceName)
        {
            return new MockContentSourceService();
        }

        public (IContentSourceService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            throw new NotImplementedException();
        }
    }

    public class ExtractionHandlerTests
    {
        [Fact]
        public async void TestProcessRequest()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["content_source_name"] = "BlobStorage",
                    ["vectorization_parameters_root:vectorization_parameters:Key1"] = "Value1"
                })
                .Build();
            IConfigurationSection stepsConfiguration = configurationRoot.GetSection("vectorization_parameters_root");
            
            // DI container configuration
            IVectorizationStateService stateService = A.Fake<IVectorizationStateService>();
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IVectorizationServiceFactory<IContentSourceService>, MockServiceFactory>();

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            ExtractionHandler handler = new ExtractionHandler(
                "Instance123",
                "Queue-Message-1",
                new Dictionary<string, string> { { "configuration_section", "vectorization_parameters" } },
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
                    new VectorizationStep { Id = "extract", Parameters = new Dictionary<string, string> { } }
                },
                CompletedSteps = new List<string> { },
                RemainingSteps = new List<string> { "extract" }
            };
            VectorizationState state = new VectorizationState
            {
                CurrentRequestId = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                ContentIdentifier = contentIdentifier
            };
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            await handler.Invoke(request, state, tokenSource.Token);

            Assert.True(state.Artifacts.First(artifact => artifact.Type == VectorizationArtifactType.ExtractedText).Content == "This is the PDF document data.");
        }
    }
}