using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for Agent-to-Agent Conversations.
    /// </summary>
    public class Example0015_AgentToAgentConversations : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IVectorizationTestService _vectorizationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileName = "indexing_profile_dune";

        public Example0015_AgentToAgentConversations(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();

        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Agent-to-Agent Conversations with SemanticKernel on Dune ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            var agentName = TestAgentNames.Dune03;
            try
            {
                var userPrompts = new List<string>
                {
                    "Who is 'Paul-Muad'Dib' and what is his relationship to the Fremen?",
                    "Write a poem about Paul's ambition."
                };

                WriteLine($"Send questions to the {agentName} agent.");

                await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);
                await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

                await _managementAPITestManager.CreateAgent(TestAgentNames.Dune01, indexingProfileName, textEmbeddingProfileName);
                await _managementAPITestManager.CreateAgent(TestAgentNames.Dune02);

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
                await _managementAPITestManager.DeleteAgent(TestAgentNames.Dune01);
                await _managementAPITestManager.DeleteAgent(TestAgentNames.Dune02);
                await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, false);
                await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);
            }
        }
    }
}
