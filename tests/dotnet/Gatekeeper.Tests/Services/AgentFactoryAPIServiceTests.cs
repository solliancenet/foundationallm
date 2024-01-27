using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Services.API;

namespace Gatekeeper.Tests.Services
{
    public class AgentFactoryAPIServiceTests
    {
        private readonly DownstreamAPIService _testedService;

        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        
        public AgentFactoryAPIServiceTests()
        {
            _testedService = new DownstreamAPIService(HttpClients.AgentHubAPI, 
                _httpClientFactoryService);
        }

        [Fact]
        public async Task GetCompletion_SuccessfulCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest { UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new CompletionResponse { Completion = "Test Completion" });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var completionResponse = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("Test Completion", completionResponse.Completion);
        }

        [Fact]
        public async Task GetCompletion_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest { UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var completionResponse = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("A problem on my side prevented me from responding.", completionResponse.Completion);
        }

        [Fact]
        public async Task GetSummary_SuccessfulCompletionResponse()
        {
            // Arrange
            var summaryRequest = new SummaryRequest { UserPrompt = "Prompt_1" };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new SummaryResponse { Summary = "Test Response" });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var summaryResponse = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
            Assert.Equal("Test Response", summaryResponse.Summary);
        }

        [Fact]
        public async Task GetSummary_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var summaryRequest = new SummaryRequest { UserPrompt = "Prompt_1" };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            // Act
            var summaryResponse = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
            Assert.Equal("[No Summary]", summaryResponse.Summary);
        }

    }
}
