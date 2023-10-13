using FoundationaLLM.Common.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class SessionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            var session = new Session();

            Assert.NotNull(session.Id);
            Assert.Equal(nameof(Session), session.Type);
            Assert.Equal(session.Id, session.SessionId);
            Assert.Equal(0, session.TokensUsed);
            Assert.Equal("New Chat", session.Name);
            Assert.NotNull(session.Messages);
            Assert.Empty(session.Messages);
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToMessagesList()
        {
            // Arrange
            var session = new Session();
            var message = new Message("1", "sender1", null, "The message", null, null);

            // Act
            session.AddMessage(message);

            // Assert
            Assert.Contains(message, session.Messages);
        }

        [Fact]
        public void UpdateMessage_ShouldUpdateExistingMessageInMessagesList()
        {
            // Arrange
            var session = new Session();
            var initialMessage = new Message("1", "sender1", null, "The message", null, null);
            session.AddMessage(initialMessage);

            var updatedMessage = new Message("1", "sender1", null, "The message updated", null, null);
            updatedMessage.Id = initialMessage.Id;

            // Act
            session.UpdateMessage(updatedMessage);

            // Assert
            Assert.DoesNotContain(initialMessage, session.Messages);
            Assert.Contains(updatedMessage, session.Messages);
        }
    }
}
