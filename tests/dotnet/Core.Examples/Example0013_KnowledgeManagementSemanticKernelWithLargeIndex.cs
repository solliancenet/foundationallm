using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for the Knowledge Management agent with SemanticKernel.
    /// </summary>
    public class Example0013_KnowledgeManagementSemanticKernelWithLargeIndex : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IVectorizationTestService _vectorizationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileName = "indexing_profile_dune";

        public Example0013_KnowledgeManagementSemanticKernelWithLargeIndex(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Knowledge Management agent with SemanticKernel on Dune ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            var agentName = TestAgentNames.Dune01;
            try
            {
                var userPrompts = new List<string>
                {
                    "Who are you?",
                    "Who is the enemy of Paul Atreides?",
                    "Who is 'Paul-Muad'Dib' and what is his relationship to the Fremen?"
                };

                WriteLine($"Send questions to the {agentName} agent.");

                await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);
                await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

                var response = await _agentConversationTestService.RunAgentConversationWithSession(
                    agentName, userPrompts, null, true, indexingProfileName, textEmbeddingProfileName);

                WriteLine($"Agent conversation history:");

                var invalidAgentResponsesFound = 0;
                foreach (var message in response)
                {
                    WriteLine($"- {message.Sender}: {message.Text}");

                    if (string.Equals(message.Sender, Common.Constants.Agents.InputMessageRoles.Assistant, StringComparison.CurrentCultureIgnoreCase) &&
                        message.Text == TestResponseMessages.FailedCompletionResponse)
                    {
                        invalidAgentResponsesFound++;
                    }
                }

                Assert.True(invalidAgentResponsesFound == 0, $"{invalidAgentResponsesFound} invalid agent responses found.");
            }
            finally
            {
                await _managementAPITestManager.DeleteAgent(agentName);
                await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, false);
                await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);
            }
        }
    }
}
