using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Runtime.Intrinsics;

namespace FoundationaLLM.Core.Tests.Services
{
    public class CoreServiceTests
    {
        private readonly CoreService _coreService;

        private readonly ICosmosDbService _cosmosDbService = Substitute.For<ICosmosDbService>();
        private readonly IGatekeeperAPIService _gatekeeperAPIService = Substitute.For<IGatekeeperAPIService>();
        private readonly ILogger<CoreService> _logger = Substitute.For<ILogger<CoreService>>();

        public CoreServiceTests()
        {
            _coreService = new CoreService(_cosmosDbService, _gatekeeperAPIService, _logger);
        }

        [Fact]
        public async Task GetAllChatSessionsAsync_ShouldReturnAllChatSessions()
        {
            // Arrange
            var expectedSessions = new List<Session>() { new Session() };
            _cosmosDbService.GetSessionsAsync().Returns(expectedSessions);

            // Act
            var actualSessions = await _coreService.GetAllChatSessionsAsync();

            // Assert
            Assert.Equivalent(expectedSessions, actualSessions);
        }


        [Fact]
        public async Task GetChatSessionMessagesAsync_ShouldReturnAllChatSessionMessages()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var message = new Message(sessionId, "sender", 0, "text", null, null);
            var expectedMessages = new List<Message>() { message };
            _cosmosDbService.GetSessionMessagesAsync(sessionId).Returns(expectedMessages);

            // Act
            var actualMessages = await _coreService.GetChatSessionMessagesAsync(sessionId);

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
                await _coreService.GetChatSessionMessagesAsync(sessionId);
            });
        }

        [Fact]
        public async Task CreateNewChatSessionAsync_ShouldReturnANewChatSession()
        {
            // Arrange
            var expectedSession = new Session();
            _cosmosDbService.InsertSessionAsync(expectedSession).Returns(expectedSession);

            // Act
            var actualSession = await _coreService.CreateNewChatSessionAsync();

            // Assert
            Assert.Equivalent(expectedSession, actualSession);
        }

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
            var actualSession = await _coreService.RenameChatSessionAsync(session.Id, expectedName);

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
                await _coreService.RenameChatSessionAsync(null, newChatSessionName);
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
                await _coreService.RenameChatSessionAsync(sessionId, null);
            });

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _coreService.RenameChatSessionAsync(sessionId, string.Empty);
            });
        }

        [Fact]
        public async Task DeleteChatSessionAsync_ShouldSucceed()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var expected = Task.CompletedTask;
            _cosmosDbService.DeleteSessionAndMessagesAsync(sessionId).Returns(expected);

            // Act
            Task actual = _coreService.DeleteChatSessionAsync(sessionId);
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
                await _coreService.DeleteChatSessionAsync(null);
            });
        }

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
            var actualCompletion = await _coreService.GetChatCompletionAsync(sessionId, userPrompt);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        public async Task GetChatCompletionAsync_ShouldReturnAnErrorMessageWhenParametersAreNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var expectedCompletion = new Completion { Text = "Could not generate a completion due to an internal error." };

            var expectedMessages = new List<Message>();
            _cosmosDbService.GetSessionMessagesAsync(sessionId).Returns(expectedMessages);

            var completionResponse = new CompletionResponse() { Completion = "Completion" };
            _gatekeeperAPIService.GetCompletion(Arg.Any<CompletionRequest>()).Returns(completionResponse);

            _cosmosDbService.GetSessionAsync(sessionId).Returns(new Session());

            _cosmosDbService.UpsertSessionBatchAsync().Returns(Task.CompletedTask);

            // Act
            var actualCompletion = await _coreService.GetChatCompletionAsync(null, null);

            // Assert
            Assert.Equal(expectedCompletion.Text, actualCompletion.Text);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenUserPromptIsNull()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();

            // Act
            var exception = Record.Exception(() => _coreService.GetChatCompletionAsync(sessionId, null).Result);

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GetChatCompletionAsync_ShouldNotThrowExceptionWhenSessionIdIsNull()
        {
            // Arrange
            var userPrompt = "Prompt";

            // Act
            var exception = Record.Exception(() => _coreService.GetChatCompletionAsync(null, userPrompt).Result);

            // Assert
            Assert.Null(exception);
        }

        //SummarizeChatSessionNameAsync

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
            var actualMessage = await _coreService.RateMessageAsync(id, sessionId, rating);

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
                await _coreService.RateMessageAsync(null, sessionId, rating);
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
                await _coreService.RateMessageAsync(id, null, rating);
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
                await _coreService.RateMessageAsync(id, sessionId, null);
            });
        }

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
            var actualPrompt = await _coreService.GetCompletionPrompt(sessionId, completionPromptId);

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
                await _coreService.GetCompletionPrompt(null, completionPromptId);
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
                await _coreService.GetCompletionPrompt(sessionId, null);
            });
        }
    }
}
