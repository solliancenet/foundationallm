using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for sending user queries to a Knowledge Management with inline context agent using the LangChain orchestrator.
    /// </summary>
    public class Example0003_KnowledgeManagementInlineContextAgentWithLangChain : BaseTest, IClassFixture<TestFixture>
	{
		private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

		public Example0003_KnowledgeManagementInlineContextAgentWithLangChain(ITestOutputHelper output, TestFixture fixture)
			: base(output, fixture.ServiceProvider)
		{
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();
		}

		[Fact]
		public async Task RunAsync()
		{
			WriteLine("============ Knowledge Management with inline context agent using LangChain ============");
			await RunExampleAsync();
		}

		private async Task RunExampleAsync()
        {
            var agentName = Constants.TestAgentNames.GenericInlineContextAgentName;
            try
            {
                var userPrompts = new List<string>
                {
                    "Who are you?",
                    "What is the significance of the Rosetta Stone in the history of linguistics?",
                    "What was the Rosetta Stone's role in ancient political dynamics?",
                    "How did the decipherment of the Rosetta Stone impact the study of ancient Egypt?"
                };

                WriteLine($"Send Rosetta Stone questions to the {agentName} agent.");

                var response = await _agentConversationTestService.RunAgentConversationWithSession(
                    agentName, userPrompts, null, true);

                WriteLine($"Agent conversation history:");
                var invalidAgentResponsesFound = 0;
                foreach (var message in response!)
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
            }
        }
	}
}
