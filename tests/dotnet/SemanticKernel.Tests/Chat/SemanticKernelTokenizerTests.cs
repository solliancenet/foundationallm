using FoundationaLLM.SemanticKernel.Core.Services;

namespace FoundationaLLM.SemanticKernel.Tests.Chat
{
    public class SemanticKernelTokenizerTests
    {
        private readonly SemanticKernelTokenizer _testedService;

        public SemanticKernelTokenizerTests()
        {
            _testedService = new SemanticKernelTokenizer();
        }

        #region GetTokensCount

        [Fact]
        public async Task GetTokensCount_ShouldReturnTheNumberOfTokensForTheInputText()
        {
            // Arrange
            var text = "Encode this text and return the number of tokens used.";
            var expected = 12;

            // Act
            var actual = _testedService.GetTokensCount(text);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetTokensCount_ShouldReturnZeroWhenInputTextIsNullOrEmpty()
        {
            // Arrange
            var expected = 0;

            // Act
            var actualEmpty = _testedService.GetTokensCount(string.Empty);
            var actualNull = _testedService.GetTokensCount(null);

            // Assert
            Assert.Equal(expected, actualEmpty);
            Assert.Equal(expected, actualNull);
        }

        #endregion
    }
}
