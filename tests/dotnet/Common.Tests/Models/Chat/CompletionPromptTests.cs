using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class CompletionPromptTests
    {
        public static IEnumerable<object[]> GetInvalidFields()
        {
            yield return new object[] { null, "Message_1", "Test prompt" };
            yield return new object[] { "Session_1",null, "Test prompt" };
            yield return new object[] { "Session_1", "Message_1", null};
        }

        public static IEnumerable<object[]> GetValidFields()
        {
            yield return new object[] { "Session_1", "Message_1", "Prompt_1"};
        }

        [Theory]
        [MemberData(nameof(GetInvalidFields))]
        public void Create_CompletionPrompt_FailsWithInvalidValues(string sessionId, string messageId, string prompt)
        {
            Assert.Throws<Exception>(() => CreateCompletionPrompt(sessionId, messageId, prompt));
        }

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_CompletionPrompt_SucceedsWithValidValues(string sessionId, string messageId, string prompt)
        {
            //Act
            var exception = Record.Exception(() => CreateCompletionPrompt(sessionId, messageId, prompt));

            //Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedSessionId = "Session_1";
            string expectedMessageId = "Message_1";
            string expectedPrompt = "Test prompt";

            // Act
            var completionPrompt = CreateCompletionPrompt(expectedSessionId, expectedMessageId, expectedPrompt);

            // Assert
            Assert.NotNull(completionPrompt.Id);
            Assert.Equal(nameof(CompletionPrompt), completionPrompt.Type);
            Assert.Equal(expectedSessionId, completionPrompt.SessionId);
            Assert.Equal(expectedMessageId, completionPrompt.MessageId);
            Assert.Equal(expectedPrompt, completionPrompt.Prompt);
        }

        public CompletionPrompt CreateCompletionPrompt(string sessionId, string messageId, string prompt)
        {
            return new CompletionPrompt(sessionId, messageId, prompt);
        }
    }
}
