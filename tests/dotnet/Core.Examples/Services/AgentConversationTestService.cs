using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.AzureAIService;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <summary>
    /// Service for running agent conversations using the Core API.
    /// </summary>
    /// <param name="coreAPITestManager"></param>
    /// <param name="azureAIService"></param>
    public class AgentConversationTestService(
        IAuthenticationService authService,
        ICoreClient coreClient,
        ICoreRESTClient coreRestClient,
        IManagementAPITestManager managementAPITestManager,
        IAzureAIService azureAIService = null) : IAgentConversationTestService
    {
        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> RunAgentConversationWithSession(
            string agentName,
            List<string> userPrompts, 
            string? sessionId = null, 
            bool createAgent = false, 
            string? indexingProfileName = null,
            string? textEmbeddingProfileName = null, 
            string? textPartitioningProfileName = null)
        {
            var sessionCreated = false;

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreClient.CreateChatSessionAsync((string?) null);
                sessionCreated = true;
            }

            if (createAgent)
            {
                // Create a new agent and its dependencies for the test.
                await managementAPITestManager.CreateAgent(agentName, indexingProfileName, textEmbeddingProfileName, textPartitioningProfileName);
            }

            // Send user prompts and agent responses.
            foreach (var userPrompt in userPrompts)
            {
                // Send the orchestration request to the Core API's session completion endpoint.
                await coreClient.GetCompletionWithSessionAsync(sessionId, null, userPrompt, agentName);
            }

            // Retrieve the messages from the chat session.
            var messages = await coreClient.GetChatSessionMessagesAsync(sessionId);

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreClient.DeleteSessionAsync(sessionId);
            }

            if (createAgent)
            {
                // Delete the agent and its dependencies.
                await managementAPITestManager.DeleteAgent(agentName);
            }

            return messages;
        }

        /// <inheritdoc/>
        public async Task<Completion> RunAgentCompletionWithSession(string agentName,
            string userPrompt, string? sessionId = null, bool createAgent = false)
        {
            var sessionCreated = false;

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreClient.CreateChatSessionAsync((string?) null);
                sessionCreated = true;
            }

            if (createAgent)
            {
                // Create a new agent and its dependencies for the test.
                await managementAPITestManager.CreateAgent(agentName);
            }

            // Send the orchestration request to the Core API's session completion endpoint.
            var completion = await coreClient.GetCompletionWithSessionAsync(sessionId, null, userPrompt, agentName);

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreClient.DeleteSessionAsync(sessionId);
            }

            if (createAgent)
            {
                // Delete the agent and its dependencies.
                await managementAPITestManager.DeleteAgent(agentName);
            }

            return completion;
        }

        /// <inheritdoc/>
        public async Task<Completion> RunAgentCompletionWithNoSession(string agentName,
            string userPrompt, bool createAgent = false)
        {
            if (createAgent)
            {
                // Create a new agent and its dependencies for the test.
                await managementAPITestManager.CreateAgent(agentName);
            }

            // Create a new orchestration request for the user prompt and chat session.
            var completionRequest = new CompletionRequest
            {
                AgentName = agentName,
                UserPrompt = userPrompt,
                Settings = null
            };

            // Send the orchestration request to the Core API's orchestration completion endpoint.
            var completion = await coreClient.GetCompletionAsync(completionRequest);

            if (createAgent)
            {
                // Delete the agent and its dependencies.
                await managementAPITestManager.DeleteAgent(agentName);
            }

            return completion;
        }

        /// <inheritdoc/>
        public async Task<CompletionQualityMeasurementOutput> RunAgentCompletionWithQualityMeasurements(string agentName,
            string userPrompt, string expectedCompletion, string? sessionId = null, bool createAgent = false)
        {
            var sessionCreated = false;
            var completionQualityMeasurementOutput = new CompletionQualityMeasurementOutput();

            if (azureAIService == null)
            {
                throw new InvalidOperationException("The Azure AI service is required for this operation. Please make sure you have configured your testsettings.json file with the CompletionQualityMeasurementConfiguration section and its AgentPrompts.");
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                // Create a new session since an existing ID was not provided.
                sessionId = await coreClient.CreateChatSessionAsync((string?) null);
                sessionCreated = true;
            }

            if (createAgent)
            {
                // Create a new agent and its dependencies for the test.
                await managementAPITestManager.CreateAgent(agentName);
            }

            // Create a new orchestration request for the user prompt and chat session.
            var orchestrationRequest = new CompletionRequest
            {
                SessionId = sessionId,
                AgentName = agentName,
                UserPrompt = userPrompt,
                Settings = null
            };

            // Send the orchestration request to the Core API's session completion endpoint.
            var completionResponse = await coreClient.GetCompletionWithSessionAsync(orchestrationRequest);

            // Retrieve the messages from the chat session.
            var messages = await coreClient.GetChatSessionMessagesAsync(sessionId);

            // Get the last message where the agent is the sender.
            var lastAgentMessage = messages.LastOrDefault(m => m.Sender == nameof(Participants.Assistant));
            if (lastAgentMessage != null && !string.IsNullOrWhiteSpace(lastAgentMessage.CompletionPromptId))
            {
                // Get the completion prompt from the last agent message.
                var completionPrompt = await coreRestClient.Sessions.GetCompletionPromptAsync(sessionId,
                    lastAgentMessage.CompletionPromptId);
                // For the context, take everything in the prompt that comes after `\\n\\nContext:\\n`. If it doesn't exist, take the whole prompt.
                var contextIndex =
                    completionPrompt.Prompt.IndexOf(@"\n\nContext:\n", StringComparison.Ordinal);
                if (contextIndex != -1)
                {
                    completionPrompt.Prompt = completionPrompt.Prompt[(contextIndex + 14)..];
                }

                var dataSet = new InputsMapping
                {
                    Question = userPrompt,
                    Answer = completionResponse?.Text,
                    Context = completionPrompt.Prompt,
                    GroundTruth = expectedCompletion,
                };
                // Create a new Azure AI evaluation from the data.
                var dataSetName = $"{agentName}_{sessionId}";
                var dataSetPath = await azureAIService.CreateDataSet(dataSet, dataSetName);
                var dataSetVersion = await azureAIService.CreateDataSetVersion(dataSetName, dataSetPath);
                _ = int.TryParse(dataSetVersion.DataVersion.VersionId, out var dataSetVersionNumber);
                var jobId = await azureAIService.SubmitJob(dataSetName, dataSetName,
                    dataSetVersionNumber == 0 ? 1 : dataSetVersionNumber,
                    string.Empty);

                completionQualityMeasurementOutput.JobID = jobId;
                completionQualityMeasurementOutput.UserPrompt = userPrompt;
                completionQualityMeasurementOutput.AgentCompletion = completionResponse?.Text;
                completionQualityMeasurementOutput.ExpectedCompletion = expectedCompletion;
            }

            // Delete the session to clean up after the test.
            if (sessionCreated)
            {
                await coreClient.DeleteSessionAsync(sessionId);
            }

            if (createAgent)
            {
                // Delete the agent and its dependencies.
                await managementAPITestManager.DeleteAgent(agentName);
            }

            return completionQualityMeasurementOutput;
        }
    }
}
