using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using Microsoft.Graph.Models.CallRecords;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace FoundationaLLM.Core.Tests.Services
{
    public class CoreServiceTests
    {
        private readonly string _instanceId = "00000000-0000-0000-0000-000000000000";
        private readonly CoreService _testedService;

        private readonly IAzureCosmosDBService _cosmosDbService = Substitute.For<IAzureCosmosDBService>();
        private readonly IGatekeeperAPIService _gatekeeperAPIService = Substitute.For<IGatekeeperAPIService>();
        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly IEnumerable<IResourceProviderService> _resourceProviderServices = Substitute.For<IEnumerable<IResourceProviderService>>();
        private readonly ILogger<CoreService> _logger = Substitute.For<ILogger<CoreService>>();
        private readonly IOptions<ClientBrandingConfiguration> _brandingConfig = Substitute.For<IOptions<ClientBrandingConfiguration>>();
        private readonly IEnumerable<IDownstreamAPIService> _downstreamAPIServices;
        private IOptions<CoreServiceSettings> _options;

        public CoreServiceTests()
        {
            var gatekeeperAPIDownstream = Substitute.For<IDownstreamAPIService>();
            gatekeeperAPIDownstream.APIName.Returns(HttpClientNames.GatekeeperAPI);

            var orchestrationAPIDownstream = Substitute.For<IDownstreamAPIService>();
            orchestrationAPIDownstream.APIName.Returns(HttpClientNames.OrchestrationAPI);

            _downstreamAPIServices = new List<IDownstreamAPIService>
            {
                gatekeeperAPIDownstream,
                orchestrationAPIDownstream
            };

            _options = Options.Create(new CoreServiceSettings {
                BypassGatekeeper =  true, 
                SessionSummarization = ChatSessionNameSummarizationType.LLM
            });

            _brandingConfig.Value.Returns(new ClientBrandingConfiguration());
            _callContext.CurrentUserIdentity.Returns(new UnifiedUserIdentity
            {
                Name = "Test User",
                UPN = "test@foundationallm.ai",
                Username = "test@foundationallm.ai"
            });
            _testedService = new CoreService(_cosmosDbService, _downstreamAPIServices, _logger, _brandingConfig, _options, _callContext, _resourceProviderServices);
        }

        #region GetAllChatSessionsAsync

        [Fact]
        public async Task GetAllChatSessionsAsync_ShouldReturnAllChatSessions()
        {
            // Arrange
            var expectedSessions = new List<Session>() { new Session() };
            _cosmosDbService.GetConversationsAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(expectedSessions);

            // Act
            var actualSessions = await _testedService.GetAllConversationsAsync(_instanceId);

            // Assert
            Assert.Equivalent(expectedSessions, actualSessions);
        }

        #endregion

        #region GetChatSessionMessagesAsync

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldReturnAllChatSessionMessages()
        {
            // Arrange
            string sessionId = Guid.NewGuid().ToString();
            var message = new Message(sessionId, "sender", 0, "text", null, null, "test_upn");
            var expectedMessages = new List<Message>() { message };

            _cosmosDbService.GetSessionMessagesAsync(sessionId, Arg.Any<string>())
                .Returns(expectedMessages);

            // Act
            var messages = await _testedService.GetChatSessionMessagesAsync(_instanceId, sessionId);

            // Assert
            Assert.Equal(expectedMessages, messages);
        }

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            string sessionId = null!;
            _cosmosDbService.GetSessionMessagesAsync(sessionId, "").ReturnsNull();

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetChatSessionMessagesAsync(_instanceId, sessionId);
            });
        }

        #endregion

        #region CreateNewChatSessionAsync

        [Fact]
        public async Task CreateNewChatSessionAsync_ShouldReturnANewChatSession()
        {
            // Arrange
            var currentUserUPN = "testuser@example.com";
            var sessionType = "Test_type";
            var chatSessionProperties = new ChatSessionProperties() { Name = "Test_name" };
            var newSession = new Session { Name = chatSessionProperties.Name, Type = sessionType, UPN = currentUserUPN };

            // Set up mock returns
            _callContext.CurrentUserIdentity.Returns(new UnifiedUserIdentity { UPN = currentUserUPN });

            _cosmosDbService.InsertSessionAsync(Arg.Any<Session>())
                .Returns(Task.FromResult(newSession));

            // Act
            var resultSession = await _testedService.CreateNewChatSessionAsync(_instanceId, chatSessionProperties);

            // Assert
            Assert.NotNull(resultSession);
            Assert.Equal(sessionType, resultSession.Type);
            Assert.Equal(currentUserUPN, resultSession.UPN);
            Assert.Equal(chatSessionProperties.Name, resultSession.Name);
        }

        #endregion

        #region RenameChatSessionAsync

        [Fact]
        public async Task RenameChatSessionAsync_ShouldReturnTheRenamedChatSession()
        {
            // Arrange
            var session = new Session() { Name = "OldName" };
            var chatSessionProperties = new ChatSessionProperties() { Name = "NewName" };

            var expectedSession = new Session()
            {
                Id = session.Id,
                Messages = session.Messages,
                Name = chatSessionProperties.Name,
                SessionId = session.SessionId,
                TokensUsed = session.TokensUsed,
                Type = session.Type,
            };
            _cosmosDbService.UpdateSessionNameAsync(session.Id, chatSessionProperties.Name).Returns(expectedSession);

            // Act
            var actualSession = await _testedService.RenameChatSessionAsync(_instanceId, session.Id, chatSessionProperties);

            // Assert
            Assert.Equivalent(expectedSession, actualSession);
            Assert.Equal(chatSessionProperties.Name, actualSession.Name);
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var chatSessionProperties = new ChatSessionProperties() { Name = "NewName" };

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>((Func<Task>)(async () =>
            {
                await _testedService.RenameChatSessionAsync(_instanceId, null!, chatSessionProperties);
            }));
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenNewChatSessionNameIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RenameConversationAsync(_instanceId, sessionId, null!);
            });

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(_instanceId, sessionId, new ChatSessionProperties() { Name = string.Empty });
            });
        }

        #endregion

        #region DeleteChatSessionAsync

        [Fact]
        public async Task DeleteChatSessionAsync_ShouldSucceed()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var expected = Task.CompletedTask;
            _cosmosDbService.DeleteConversationAsync(sessionId).Returns(expected);

            // Act
            Task actual = _testedService.DeleteConversationAsync(_instanceId, sessionId);
            await actual;

            // Assert
            Assert.True(actual.IsCompletedSuccessfully);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DeleteChatSessionAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.DeleteConversationAsync(_instanceId, null!);
            });
        }

        #endregion

        #region GetChatCompletionAsync

        [Fact]
        public async Task GetChatCompletionAsync_ShouldReturnACompletion()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var userPrompt = "Prompt";
            var orchestrationRequest = new CompletionRequest { SessionId = sessionId, UserPrompt = userPrompt };
            var upn = "test@foundationallm.ai";
            var expectedCompletion = new Completion() { Text = "Completion" };

            var expectedMessages = new List<Message>();
            _cosmosDbService.GetSessionMessagesAsync(sessionId, upn).Returns(expectedMessages);

            var completionResponse = new CompletionResponse() { Completion = "Completion" };
            _downstreamAPIServices.Last().GetCompletion(_instanceId, Arg.Any<CompletionRequest>()).Returns(completionResponse);

            _cosmosDbService.GetConversationAsync(sessionId).Returns(new Session());
            _cosmosDbService.UpsertSessionBatchAsync().Returns(Task.CompletedTask);

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(_instanceId, orchestrationRequest);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldReturnAnErrorMessageWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";
            var orchestrationRequest = new CompletionRequest { UserPrompt = userPrompt };
            var expectedCompletion = new Completion { Text = "Could not generate a completion due to an internal error." };

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(_instanceId, orchestrationRequest);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);

            //_logger.Received(1).LogError($"Error getting completion in session {sessionId} for user prompt [{userPrompt}].");
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenUserPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var orchestrationRequest = new CompletionRequest { SessionId = sessionId, UserPrompt = null! };

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GetChatCompletionAsync(_instanceId, orchestrationRequest));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";
            var orchestrationRequest = new CompletionRequest { UserPrompt = userPrompt };

            // Act
            var exception = await Record.ExceptionAsync(async () => await _testedService.GetChatCompletionAsync(_instanceId, orchestrationRequest));

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region RateMessageAsync

        [Fact]
        public async Task RateMessageAsync_ShouldReturnARatedMessage()
        {
            // Arrange
            var rating = true;
            var id = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();
            var upn = "";
            var expectedMessage = new Message(sessionId, string.Empty, default, "Text", null, rating, upn);
            _cosmosDbService.UpdateMessageRatingAsync(id, sessionId, rating).Returns(expectedMessage);

            // Act
            var actualMessage = await _testedService.RateMessageAsync(_instanceId, id, sessionId, rating);

            // Assert
            Assert.Equivalent(expectedMessage, actualMessage);
        }

        [Fact]
        public async Task RateMessageAsync_ShouldThrowExceptionWhenIdIsNull()
        {
            // Arrange
            var rating = true;
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RateMessageAsync(_instanceId, null!, sessionId, rating);
            });
        }

        [Fact]
        public async Task RateMessageAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var rating = true;
            var id = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RateMessageAsync(_instanceId, id, null!, rating);
            });
        }

        #endregion

        #region GetCompletionPrompt

        [Fact]
        public async Task GetCompletionPrompt_ShouldReturnACompletionPrompt()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var messageId = Guid.NewGuid().ToString();
            var completionPromptId = Guid.NewGuid().ToString();
            var expectedPrompt = new CompletionPrompt(sessionId, messageId, "Text");
            _cosmosDbService.GetCompletionPromptAsync(sessionId, completionPromptId).Returns(expectedPrompt);

            // Act
            var actualPrompt = await _testedService.GetCompletionPrompt(_instanceId, sessionId, completionPromptId);

            // Assert
            Assert.Equivalent(actualPrompt, expectedPrompt);
        }

        [Fact]
        public async Task GetCompletionPrompt_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var completionPromptId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetCompletionPrompt(_instanceId, null!, completionPromptId);
            });
        }

        [Fact]
        public async Task GetCompletionPrompt_ShouldThrowExceptionWhenCompletionPromptIdIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetCompletionPrompt(_instanceId, sessionId, null!);
            });
        }

        #endregion
    }
}
