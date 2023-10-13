using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class MessageHistoryItemTests
    {
        public static IEnumerable<object[]> GetInvalidFields()
        {
            yield return new object[] { null, "Test"};
            yield return new object[] { "Sender_1", null};
        }

        public static IEnumerable<object[]> GetValidFields()
        {
            yield return new object[] { "Sender_1", "Test" };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFields))]
        public void Create_MessageHistoryItem_FailsWithInvalidValues(string sender, string text)
        {
            Assert.Throws<Exception>(() => CreateMessageHistoryItem(sender,text));
        }

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_MessageHistoryItem_SucceedsWithValidValues(string sender, string text)
        {
            //Act
            var exception = Record.Exception(() => CreateMessageHistoryItem(sender,text));

            //Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedSender = "Sender_1";
            string expectedText = "Test";

            // Act
            var messageHistoryItem = CreateMessageHistoryItem(expectedSender, expectedText);

            // Assert
            Assert.Equal(expectedSender, messageHistoryItem.Sender);
            Assert.Equal(expectedText, messageHistoryItem.Text);
        }

        public MessageHistoryItem CreateMessageHistoryItem(string sender, string text)
        {
            return new MessageHistoryItem(sender, text);
        }
    }
}
