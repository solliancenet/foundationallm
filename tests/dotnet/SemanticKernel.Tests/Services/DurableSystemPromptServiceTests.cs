using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundationaLLM.SemanticKernel.Tests.Services
{
    public class DurableSystemPromptServiceTests
    {
        private readonly DurableSystemPromptService _testedService;

        private readonly IOptions<DurableSystemPromptServiceSettings> _settings = Substitute.For<IOptions<DurableSystemPromptServiceSettings>>();

        public DurableSystemPromptServiceTests()
        {
            _testedService = new DurableSystemPromptService(_settings);
        }

        #region GetPrompt

        [Fact]
        public async Task GetPrompt_ShouldReturnThePromptText()
        {
            // Arrange
            var expected = "promptText";
            var promptName = "promptName";

            // Act
            var actual = await _testedService.GetPrompt(promptName);

            // Assert
            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public async Task GetPrompt_ShouldThrowExceptionWhenPromptNameIsInvalid()
        {
            // Arrange
            var promptName = string.Empty;

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _testedService.GetPrompt(promptName);
            });
        }

        #endregion
    }
}
