using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class CompletionTests
    {
        [Fact]
        public void TextProperty_SetAndGet_ShouldWork()
        {
            //Arrange
            string expectedText = "CompletionText";
            var completion = new Completion();

            //Act
            completion.Text = "CompletionText";
            string actualText = completion.Text;

            //Assert
            Assert.Equal(expectedText, actualText);
        }
    }
}
