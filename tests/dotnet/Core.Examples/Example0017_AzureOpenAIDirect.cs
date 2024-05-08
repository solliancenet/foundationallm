using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running the default FoundationaLLM agent completions in both session and sessionless modes.
    /// </summary>
    public class Example0017_AzureOpenAIDirect : BaseTest, IClassFixture<TestFixture>
	{
		private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

		public Example0017_AzureOpenAIDirect(ITestOutputHelper output, TestFixture fixture)
			: base(output, fixture.ServiceProvider)
		{
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();
		}

		[Fact]
		public async Task RunAsync()
		{
			WriteLine("============ FoundationaLLM Azure OpenAI Direct ============");
			await RunExampleAsync();
		}

		private async Task RunExampleAsync()
        {
            var userPrompt = "Who are you?";
            var agentName = Constants.TestAgentNames.AzureOpenAIDirectInlineContextAgentName;

            WriteLine($"Creating new AzureOpenAIDirect Agent: {agentName}");
            await _managementAPITestManager.CreateAgent(agentName);

            WriteLine($"Send session-based \"{userPrompt}\" user prompt to the {agentName} agent.");
            var response = await _agentConversationTestService.RunAgentCompletionWithSession(agentName, userPrompt);
            WriteLine($"Agent completion response: {response.Text}");
            Assert.False(string.IsNullOrWhiteSpace(response.Text) || response.Text == Constants.TestResponseMessages.FailedCompletionResponse);
            
            WriteLine($"Send sessionless \"{userPrompt}\" user prompt to the {agentName} agent.");
            response = await _agentConversationTestService.RunAgentCompletionWithNoSession(agentName, userPrompt);
            WriteLine($"Agent completion response: {response.Text}");
            Assert.False(string.IsNullOrWhiteSpace(response.Text) || response.Text == Constants.TestResponseMessages.FailedCompletionResponse);
        }
	}
}
