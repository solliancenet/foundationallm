using FoundationaLLM.SemanticKernel.Core.Services;

namespace FoundationaLLM.SemanticKernel.Tests.Services
{
    public class InMemorySystemPromptServiceTests
    {
        private readonly InMemorySystemPromptService _testedService;

        public InMemorySystemPromptServiceTests()
        {
            _testedService = new InMemorySystemPromptService();
        }

        #region GetPrompt

        [Fact]
        public async Task GetPrompt_ShouldReturnThePromptText()
        {
            // Arrange
            var promptName = "Summarizer.TwoWords";
            var expected = @"
Summarize this prompt in one or two words to use as a label in a button on a web page. Output words only.".ReplaceLineEndings("\n");

            // Act
            var actual = await _testedService.GetPrompt(promptName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPrompt_ShouldThrowExceptionWhenPromptNameIsInvalid()
        {
            // Arrange
            var promptName = string.Empty;

            // Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _testedService.GetPrompt(promptName);
            });
        }

        #endregion
    }
}
