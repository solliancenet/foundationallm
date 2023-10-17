using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace FoundationaLLM.Core.Tests.Services
{
    public class CoreServiceTests
    {
        private readonly CoreService _testedService;

        private readonly ICosmosDbService _cosmosDbService = Substitute.For<ICosmosDbService>();
        private readonly IGatekeeperAPIService _gatekeeperAPIService = Substitute.For<IGatekeeperAPIService>();
        private readonly ILogger<CoreService> _logger = Substitute.For<ILogger<CoreService>>();

        public CoreServiceTests()
        {
            _testedService = new CoreService(_cosmosDbService, _gatekeeperAPIService, _logger);
        }

        #region GetAllChatSessionsAsync

        [Fact]
        public async Task GetAllChatSessionsAsync_ShouldReturnAllChatSessions()
        {
            // Arrange
            var expectedSessions = new List<Session>() { new Session() };
            _cosmosDbService.GetSessionsAsync().Returns(expectedSessions);

            // Act
            var actualSessions = await _testedService.GetAllChatSessionsAsync();

            // Assert
            Assert.Equivalent(expectedSessions, actualSessions);
        }

        #endregion

        #region GetChatSessionMessagesAsync

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldReturnAllChatSessionMessages()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var message = new Message(sessionId, "sender", 0, "text", null, null);
            var expectedMessages = new List<Message>() { message };
            _cosmosDbService.GetSessionMessagesAsync(sessionId).Returns(expectedMessages);

            // Act
            var actualMessages = await _testedService.GetChatSessionMessagesAsync(sessionId);

            // Assert
            Assert.Equivalent(expectedMessages, actualMessages);
        }

        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            string sessionId = null;
            _cosmosDbService.GetSessionMessagesAsync(sessionId).ReturnsNull();

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetChatSessionMessagesAsync(sessionId);
            });
        }

        #endregion

        #region CreateNewChatSessionAsync

        [Fact]
        public async Task CreateNewChatSessionAsync_ShouldReturnANewChatSession()
        {
            // Arrange
            var expectedSession = new Session();
            _cosmosDbService.InsertSessionAsync(expectedSession).Returns(expectedSession);

            // Act
            var actualSession = await _testedService.CreateNewChatSessionAsync();

            // Assert
            Assert.Equivalent(expectedSession, actualSession);
        }

        #endregion

        #region RenameChatSessionAsync

        [Fact]
        public async Task RenameChatSessionAsync_ShouldReturnTheRenamedChatSession()
        {
            // Arrange
            var session = new Session() { Name = "OldName" };
            var expectedName = "NewName";

            var expectedSession = new Session()
            {
                Id = session.Id,
                Messages = session.Messages,
                Name = expectedName,
                SessionId = session.SessionId,
                TokensUsed = session.TokensUsed,
                Type = session.Type,
            };
            _cosmosDbService.UpdateSessionNameAsync(session.Id, expectedName).Returns(expectedSession);

            // Act
            var actualSession = await _testedService.RenameChatSessionAsync(session.Id, expectedName);

            // Assert
            Assert.Equivalent(expectedSession, actualSession);
            Assert.Equal(expectedName, actualSession.Name);
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var newChatSessionName = "NewName";

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(null, newChatSessionName);
            });
        }

        [Fact]
        public async Task RenameChatSessionAsync_ShouldThrowExceptionWhenNewChatSessionNameIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(sessionId, null);
            });

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _testedService.RenameChatSessionAsync(sessionId, string.Empty);
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
            _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId).Returns(expected);

            // Act
            Task actual = _testedService.DeleteChatSessionAsync(sessionId);
            actual.Wait();

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
                await _testedService.DeleteChatSessionAsync(null);
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
            var expectedCompletion = new Completion() { Text = "Completion" };

            var expectedMessages = new List<Message>();
            _cosmosDbService.GetSessionMessagesAsync(sessionId).Returns(expectedMessages);

            var completionResponse = new CompletionResponse() { Completion = "Completion" };
            _gatekeeperAPIService.GetCompletion(Arg.Any<CompletionRequest>()).Returns(completionResponse);

            _cosmosDbService.GetSessionAsync(sessionId).Returns(new Session());

            _cosmosDbService.UpsertSessionBatchAsync().Returns(Task.CompletedTask);

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(sessionId, userPrompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldReturnAnErrorMessageWhenSessionIdIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var userPrompt = "Prompt";
            var expectedCompletion = new Completion { Text = "Could not generate a completion due to an internal error." };

            // Act
            var actualCompletion = await _testedService.GetChatCompletionAsync(null, userPrompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);

            //_logger.Received(1).LogError($"Error getting completion in session {sessionId} for user prompt [{userPrompt}].");
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenUserPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var exception = Record.Exception(() => _testedService.GetChatCompletionAsync(sessionId, null).Result);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";

            // Act
            var exception = Record.Exception(() => _testedService.GetChatCompletionAsync(null, userPrompt).Result);

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region SummarizeChatSessionNameAsync

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldReturnACompletion()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var prompt = "Prompt";
            var summary = "Summary";
            var expectedCompletion = new Completion() { Text = summary };

            _gatekeeperAPIService.GetSummary(prompt).Returns(summary);
            _cosmosDbService.UpdateSessionNameAsync(sessionId, summary).Returns(new Session());

            // Act
            var actualCompletion = await _testedService.SummarizeChatSessionNameAsync(sessionId, prompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldReturnAnErrorMessageWhenSessionIdIsNull()
        {
            // Arrange
            var prompt = "Prompt";
            var expectedCompletion = new Completion { Text = "[No Summary]" };

            // Act
            var actualSummary = await _testedService.SummarizeChatSessionNameAsync(null, prompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualSummary.Text);

            //_logger.Received(1).LogError($"Error getting a summary in session {sessionId} for user prompt [{prompt}].");
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldNotThrowExceptionWhenPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var exception = Record.Exception(() => _testedService.SummarizeChatSessionNameAsync(sessionId, null).Result);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SummarizeChatSessionNameAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var prompt = "Prompt";

            // Act
            var exception = Record.Exception(() => _testedService.SummarizeChatSessionNameAsync(null, prompt).Result);

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
            var expectedMessage = new Message(sessionId, string.Empty, default, "Text", null, rating);
            _cosmosDbService.UpdateMessageRatingAsync(id, sessionId, rating).Returns(expectedMessage);

            // Act
            var actualMessage = await _testedService.RateMessageAsync(id, sessionId, rating);

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
                await _testedService.RateMessageAsync(null, sessionId, rating);
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
                await _testedService.RateMessageAsync(id, null, rating);
            });
        }

        [Fact]
        public async Task RateMessageAsync_ShouldThrowExceptionWhenRatingIsNull()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.RateMessageAsync(id, sessionId, null);
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
            _cosmosDbService.GetCompletionPrompt(sessionId, completionPromptId).Returns(expectedPrompt);

            // Act
            var actualPrompt = await _testedService.GetCompletionPrompt(sessionId, completionPromptId);

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
                await _testedService.GetCompletionPrompt(null, completionPromptId);
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
                await _testedService.GetCompletionPrompt(sessionId, null);
            });
        }

        #endregion
    }
}
