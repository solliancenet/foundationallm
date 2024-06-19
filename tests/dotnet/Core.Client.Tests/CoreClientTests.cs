using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders;
using NSubstitute;

namespace FoundationaLLM.Client.Core.Tests
{
    public class CoreClientTests
    {
        private readonly ICoreRESTClient _coreRestClient;
        private readonly CoreClient _coreClient;

        public CoreClientTests()
        {
            _coreRestClient = Substitute.For<ICoreRESTClient>();
            _coreClient = new CoreClient();
            _coreClient.GetType().GetField("_coreRestClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_coreClient, _coreRestClient);
        }

        [Fact]
        public async Task CreateChatSessionAsync_WithName_CreatesAndRenamesSession()
        {
            // Arrange
            var sessionName = "TestSession";
            var sessionId = "session-id";
            _coreRestClient.Sessions.CreateSessionAsync().Returns(Task.FromResult(sessionId));

            // Act
            var result = await _coreClient.CreateChatSessionAsync(sessionName);

            // Assert
            Assert.Equal(sessionId, result);
            await _coreRestClient.Sessions.Received(1).CreateSessionAsync();
            await _coreRestClient.Sessions.Received(1).RenameChatSession(sessionId, sessionName);
        }

        [Fact]
        public async Task GetCompletionWithSessionAsync_WithNewSession_CreatesSessionAndSendsCompletion()
        {
            // Arrange
            var userPrompt = "Hello, World!";
            var agentName = "TestAgent";
            var sessionId = "new-session-id";
            var completion = new Completion();
            _coreRestClient.Sessions.CreateSessionAsync().Returns(Task.FromResult(sessionId));
            _coreRestClient.Completions.GetChatCompletionAsync(Arg.Any<CompletionRequest>()).Returns(Task.FromResult(completion));

            // Act
            var result = await _coreClient.GetCompletionWithSessionAsync(null, "NewSession", userPrompt, agentName);

            // Assert
            Assert.Equal(completion, result);
            await _coreRestClient.Sessions.Received(1).CreateSessionAsync();
            await _coreRestClient.Completions.GetChatCompletionAsync(Arg.Is<CompletionRequest>(
                r => r.SessionId == sessionId && r.AgentName == agentName && r.UserPrompt == userPrompt));
        }

        [Fact]
        public async Task SendSessionlessCompletionAsync_ValidRequest_SendsCompletion()
        {
            // Arrange
            var userPrompt = "Hello, World!";
            var agentName = "TestAgent";
            var completion = new Completion();
            _coreRestClient.Completions.GetChatCompletionAsync(Arg.Any<CompletionRequest>()).Returns(Task.FromResult(completion));

            // Act
            var result = await _coreClient.GetCompletionAsync(userPrompt, agentName);

            // Assert
            Assert.Equal(completion, result);
            await _coreRestClient.Completions.Received(1).GetChatCompletionAsync(Arg.Is<CompletionRequest>(
                r => r.AgentName == agentName && r.UserPrompt == userPrompt));
        }

        [Fact]
        public async Task SendSessionlessCompletionAsync_ThrowsException_WhenCompletionRequestIsInvalid()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = string.Empty
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _coreClient.GetCompletionAsync(completionRequest));
            Assert.Equal("The completion request must contain an AgentName and UserPrompt at a minimum.", ex.Message);
        }

        [Fact]
        public async Task AttachFileAndAskQuestionAsync_UsesSession_UploadsFileAndSendsSessionCompletion()
        {
            // Arrange
            var fileStream = new MemoryStream();
            var fileName = "test.txt";
            var contentType = "text/plain";
            var agentName = "TestAgent";
            var question = "What is this file about?";
            var sessionId = "session-id";
            var objectId = "object-id";
            var completion = new Completion();
            _coreRestClient.Attachments.UploadAttachmentAsync(fileStream, fileName, contentType).Returns(Task.FromResult(objectId));
            _coreRestClient.Sessions.CreateSessionAsync().Returns(Task.FromResult(sessionId));
            _coreRestClient.Completions.GetChatCompletionAsync(Arg.Any<CompletionRequest>()).Returns(Task.FromResult(completion));

            // Act
            var result = await _coreClient.AttachFileAndAskQuestionAsync(fileStream, fileName, contentType, agentName, question, true, null, "NewSession");

            // Assert
            Assert.Equal(completion, result);
            await _coreRestClient.Attachments.Received(1).UploadAttachmentAsync(fileStream, fileName, contentType);
            await _coreRestClient.Sessions.Received(1).CreateSessionAsync();
            await _coreRestClient.Completions.GetChatCompletionAsync(Arg.Is<CompletionRequest>(
                r => r.AgentName == agentName && r.SessionId == sessionId && r.UserPrompt == question && r.Attachments.Contains(objectId)));
        }

        [Fact]
        public async Task AttachFileAndAskQuestionAsync_ThrowsException_WhenFileStreamIsNull()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _coreClient.AttachFileAndAskQuestionAsync(null!, "file.txt", "text/plain", "agent", "question", true, "session-id", "session-name"));
        }

        [Fact]
        public async Task GetChatSessionMessagesAsync_ValidRequest_ReturnsMessages()
        {
            // Arrange
            var sessionId = "session-id";
            var messages = new List<Message> { new Message(sessionId, "TestSender", null, "Hello", null, null, "test@foundationallm.ai") };
            _coreRestClient.Sessions.GetChatSessionMessagesAsync(sessionId).Returns(Task.FromResult<IEnumerable<Message>>(messages));

            // Act
            var result = await _coreClient.GetChatSessionMessagesAsync(sessionId);

            // Assert
            Assert.Equal(messages, result);
            await _coreRestClient.Sessions.Received(1).GetChatSessionMessagesAsync(sessionId);
        }

        [Fact]
        public async Task GetAgentsAsync_ValidRequest_ReturnsAgents()
        {
            // Arrange
            var agents = new List<ResourceProviderGetResult<AgentBase>> { new ResourceProviderGetResult<AgentBase>
                {
                    Resource = new KnowledgeManagementAgent
                    {
                        Name = "TestAgent",
                        Description = "Test Agent Description"
                    },
                    Actions = [],
                    Roles = []
                }
            };
            _coreRestClient.Completions.GetAgentsAsync().Returns(Task.FromResult<IEnumerable<ResourceProviderGetResult<AgentBase>>>(agents));

            // Act
            var result = await _coreClient.GetAgentsAsync();

            // Assert
            Assert.Equal(agents, result);
            await _coreRestClient.Completions.Received(1).GetAgentsAsync();
        }

        [Fact]
        public async Task DeleteSessionAsync_ValidRequest_DeletesSession()
        {
            // Arrange
            var sessionId = "session-id";

            // Act
            await _coreClient.DeleteSessionAsync(sessionId);

            // Assert
            await _coreRestClient.Sessions.Received(1).DeleteSessionAsync(sessionId);
        }
    }
}