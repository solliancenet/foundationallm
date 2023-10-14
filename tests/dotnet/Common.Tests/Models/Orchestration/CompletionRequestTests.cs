using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class CompletionRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedPrompt = "Generate some text";
            var expectedMessageHistory = new List<MessageHistoryItem>
            {
                new MessageHistoryItem("Sender_1", "Test"),
                new MessageHistoryItem("Sender_2", "Test")
            };

            // Act
            var completionRequest = new CompletionRequest
            {
                Prompt = expectedPrompt,
                MessageHistory = expectedMessageHistory
            };

            // Assert
            Assert.Equal(expectedPrompt, completionRequest.Prompt);
            Assert.Equal(expectedMessageHistory, completionRequest.MessageHistory);
        }
    }
}
