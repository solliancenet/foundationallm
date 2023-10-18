using FoundationaLLM.SemanticKernel.Chat;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using NSubstitute;

namespace FoundationaLLM.SemanticKernel.Tests.Chat
{
    public class ChatBuilderTests
    {
        private readonly ChatBuilder _testedService;

        private readonly IKernel _kernel = Substitute.For<IKernel>();
        private readonly int _maxTokens = 500;
        private readonly Dictionary<string, Type> _memoryTypes = Substitute.For<Dictionary<string, Type>>(); //ModelRegistry.Models.ToDictionary(m => m.Key, m => m.Value.Type);
        private readonly ITokenizer? _tokenizer = Substitute.For<ITokenizer?>();
        private readonly PromptOptimizationSettings? _promptOptimizationSettings = Substitute.For<PromptOptimizationSettings?>();

        public ChatBuilderTests()
        {
            _testedService = new ChatBuilder(_kernel, _maxTokens, _memoryTypes, _tokenizer, _promptOptimizationSettings);
        }

        #region Build

        [Fact]
        public async Task Build_ShouldReturnTheChatHistory()
        {
            // Arrange
            var expected = new ChatHistory();

            var chatCompletion = Substitute.For<IChatCompletion>();
            chatCompletion.CreateNewChat().Returns(expected);

            // Act
            var actual = _testedService.Build();

            // Assert
            Assert.Equivalent(expected, actual);
        }

        #endregion
    }
}
