using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Services;
using FoundationaLLM.TestUtils.Helpers;
using NSubstitute;
using System.Net;

namespace Gatekeeper.Tests.Services
{
    public class GatekeeperIntegrationAPIServiceTests
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();

        [Fact]
        public async Task AnalyzeText_SuccessfulRequest_ReturnsAnalysisResults()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new List<string> { "Test_Analyze"});

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new GatekeeperIntegrationAPIService(_httpClientFactoryService);

            // Act
            var result = await service.AnalyzeText("Test_Analyze");

            // Assert
            Assert.Equal(new List<string> { "Test_Analyze" }, result);
        }

        [Fact]
        public async Task AnalyzeText_UnsuccessfulRequest_ReturnsEmptyList()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, new List<string> { });

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new GatekeeperIntegrationAPIService(_httpClientFactoryService);

            // Act
            var result = await service.AnalyzeText("Test_Analyze");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AnonymizeText_SuccessfulRequest_ReturnsAnonymizedText()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, new StringContent("anonymized text"));

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);
           
            var service = new GatekeeperIntegrationAPIService(_httpClientFactoryService);

            // Act
            var result = await service.AnonymizeText("Test_anonymized");

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task AnonymizeText_UnsuccessfulRequest_ReturnsErrorMessage()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "");

            var httpClient = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Arg.Any<string>()).Returns(httpClient);

            var service = new GatekeeperIntegrationAPIService(_httpClientFactoryService);

            // Act
            var result = await service.AnonymizeText("Test_anonymize");

            // Assert
            Assert.Equal("A problem on my side prevented me from responding.", result);
        }
    }
}
