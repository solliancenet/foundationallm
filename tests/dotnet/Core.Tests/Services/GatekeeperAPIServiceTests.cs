using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace FoundationaLLM.Core.Tests.Services
{
    public class GatekeeperAPIServiceTests
    {
        private readonly GatekeeperAPIService _testedService;

        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly IUserIdentityContext _userIdentityContext = Substitute.For<IUserIdentityContext>();

        public GatekeeperAPIServiceTests()
        {
            _testedService = new GatekeeperAPIService(_httpClientFactoryService, _userIdentityContext);
        }

        #region GetCompletion

        [Fact]
        public async Task GetCompletion_SuccessfulCompletionResponse()
        {
            // Arrange
            var expected = new CompletionResponse { Completion = "Test Completion" };
            var completionRequest = new CompletionRequest { UserPrompt = "Test Prompt", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, expected);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public async Task GetCompletion_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var expected = new CompletionResponse { Completion = "A problem on my side prevented me from responding." };
            var completionRequest = new CompletionRequest { UserPrompt = "Test Prompt", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected.Completion, actual.Completion);
        }

        #endregion

        #region GetSummary

        [Fact]
        public async Task GetSummary_SuccessfulCompletionResponse()
        {
            // Arrange
            var expected = "Test Response";
            var response = new SummaryResponse { Info = expected };
            var summaryRequest = "Test Prompt";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetSummary_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var expected = "[No Summary]";
            var summaryRequest = "Test Prompt";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var actual = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region SetLLMOrchestrationPreference

        [Fact]
        public async Task SetLLMOrchestrationPreference_SuccessfulCompletionResponse()
        {
            // Arrange
            var expected = true;
            string orchestrationServiceString = "Test Service";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, expected);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            bool actual = await _testedService.SetLLMOrchestrationPreference(orchestrationServiceString);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task SetLLMOrchestrationPreference_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var expected = false;
            string orchestrationServiceString = "Test Service";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, expected);
            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            bool actual = await _testedService.SetLLMOrchestrationPreference(orchestrationServiceString);

            // Assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region AddMemory

        [Fact]
        public async Task AddMemory_ShouldNotThrowException()
        {
            // Arrange
            var item = new { Prompt = "Test Prompt" };
            var itemName = "Prompt Name";
            var vectorizer = new Action<object, float[]>((obj, flt) => { });

            //Act
            var exception = Record.ExceptionAsync(async () => await _testedService.AddMemory(item, itemName, vectorizer));

            //Assert
            Assert.Null(exception);
        }

        #endregion

        #region RemoveMemory

        [Fact]
        public async Task RemoveMemory_ShouldNotThrowException()
        {
            // Arrange
            var item = new { Prompt = "Test Prompt" };

            //Act
            var exception = Record.ExceptionAsync(async () => await _testedService.RemoveMemory(item));

            //Assert
            Assert.Null(exception);
        }

        #endregion

    }
}
