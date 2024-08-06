using FakeItEasy;
using FoundationaLLM.Vectorization.Handlers;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Handlers
{
    public class VectorizationStepHandlerFactoryTests
    {
        [Fact]
        public void TestCreateStepHandler()
        {
            string messageId = "Some-Message-Name";
            string instanceId = "Instance123";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            IConfigurationSection stepsConfiguration = A.Fake<IConfigurationSection>();
            IVectorizationStateService stateService = A.Fake<IVectorizationStateService>();
            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            ILoggerFactory loggerFactory = A.Fake<ILoggerFactory>();

            string step = "extract";
            Assert.IsType<ExtractionHandler>(
                VectorizationStepHandlerFactory.Create(
                    instanceId,
                    step,
                    messageId,
                    parameters,
                    stepsConfiguration,
                    stateService,
                    serviceProvider,
                    loggerFactory
                )
            );

            step = "partition";
            Assert.IsType<PartitionHandler>(
                VectorizationStepHandlerFactory.Create(
                    instanceId,
                    step,
                    messageId,
                    parameters,
                    stepsConfiguration,
                    stateService,
                    serviceProvider,
                    loggerFactory
                )
            );

            step = "embed";
            Assert.IsType<EmbeddingHandler>(
                VectorizationStepHandlerFactory.Create(
                    instanceId,
                    step,
                    messageId,
                    parameters,
                    stepsConfiguration,
                    stateService,
                    serviceProvider,
                    loggerFactory
                )
            );

            step = "index";
            Assert.IsType<IndexingHandler>(
                VectorizationStepHandlerFactory.Create(
                    instanceId,
                    step,
                    messageId,
                    parameters,
                    stepsConfiguration,
                    stateService,
                    serviceProvider,
                    loggerFactory
                )
            );
        }
    }
}
