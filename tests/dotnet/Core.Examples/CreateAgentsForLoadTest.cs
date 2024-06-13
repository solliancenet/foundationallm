using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Creates agents and Vectorization profiles for the load test pipeline.
    /// </summary>
    public class CreateAgentsForLoadTest : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IVectorizationTestService _vectorizationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileDune = "indexing_profile_dune";
        private string indexingProfileSDZWA = "indexing_profile_sdzwa";

        public CreateAgentsForLoadTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Create Agents for Load Test ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            await _vectorizationTestService.CreateIndexingProfile(indexingProfileDune);
            await _vectorizationTestService.CreateIndexingProfile(indexingProfileSDZWA);
            await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

            await _managementAPITestManager.CreateAgent(TestAgentNames.SemanticKernelDune, indexingProfileDune, textEmbeddingProfileName);
            await _managementAPITestManager.CreateAgent(TestAgentNames.LangChainSDZWA, indexingProfileSDZWA, textEmbeddingProfileName);
        }
    }
}
