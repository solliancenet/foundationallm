using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace Gatekeeper.Tests.Services
{
    public class AgentFactoryAPIServiceTests
    {
        [Fact]
        public async Task GetCompletion_SuccessfulCompletionResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            var completionRequest = new CompletionRequest { UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new CompletionResponse { Completion = "Test Completion" });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            var completionResponse = await service.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("Test Completion", completionResponse.Completion);
        }

        [Fact]
        public async Task GetCompletion_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            var completionRequest = new CompletionRequest { UserPrompt = "Prompt_1", MessageHistory = new List<MessageHistoryItem>() };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            var completionResponse = await service.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(completionResponse);
            Assert.Equal("A problem on my side prevented me from responding.", completionResponse.Completion);
        }

        [Fact]
        public async Task GetSummary_SuccessfulCompletionResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            var summaryRequest = new SummaryRequest { Prompt = "Prompt_1" };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new SummaryResponse { Info = "Test Response" });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            var summaryResponse = await service.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
            Assert.Equal("Test Response", summaryResponse.Info);
        }

        [Fact]
        public async Task GetSummary_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            var summaryRequest = new SummaryRequest { Prompt = "Prompt_1" };

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            var summaryResponse = await service.GetSummary(summaryRequest);

            // Assert
            Assert.NotNull(summaryResponse);
            Assert.Equal("[No Summary]", summaryResponse.Info);
        }

        [Fact]
        public async Task SetLLMOrchestrationPreference_SuccessfulCompletionResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            string orchestrationServiceString = "Service_1";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, true);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            bool result = await service.SetLLMOrchestrationPreference(orchestrationServiceString);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SetLLMOrchestrationPreference_UnsuccessfulDefaultResponse()
        {
            // Arrange
            var httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
            string orchestrationServiceString = "Service_1";

            // Create a mock message handler
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, false);

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new AgentFactoryAPIService(httpClientFactoryService);

            // Act
            bool result = await service.SetLLMOrchestrationPreference(orchestrationServiceString);

            // Assert
            Assert.False(result);
        }
    }
}
