using FoundationaLLM.Common.Models.Chat;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class MessageTests
    {
        public static IEnumerable<object[]> GetInvalidFields()
        {
            yield return new object[] { null, "sender1", null, "The message", null, null };
            yield return new object[] { "", "sender1", null, "The message", null, null };
            yield return new object[] { "1", null, null, "The message", null, null };
            yield return new object[] { "1", "", null, "The message", null, null };
            yield return new object[] { "1", "sender1", null, null, null, null };
            yield return new object[] { "1", "sender1", null, "", null, null };
            yield return new object[] { "1", "sender1", null, "The message", new float[0], null };
            yield return new object[] { "1", "sender1", null, "The message", new float[] { 1, 2, 3 }, null };
        }

        public static IEnumerable<object[]> GetValidFields()
        {
            yield return new object[] { "1", "sender1", null, "The message", null, null };
            yield return new object[] { "1", "sender1", null, "The message", Enumerable.Range(0, 1536).Select(x => (float)x).ToArray(), null };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFields))]
        public void Create_Message_FailsWithInvalidValues(string sessionId, string sender, int? tokens, string text, float[]? vector, bool? rating)
        {
            Assert.Throws<Exception>(() => CreateMessage(sessionId, sender, tokens, text, vector, rating));
        }

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_Message_SucceedsWithValidValues(string sessionId, string sender, int? tokens, string text, float[]? vector, bool? rating)
        {
            //Act
            var exception = Record.Exception(() => CreateMessage(sessionId, sender, tokens, text, vector, rating));

            //Assert
            Assert.Null(exception);
        }

        public void CreateMessage(string sessionId, string sender, int? tokens, string text, float[]? vector, bool? rating)
        {
            var message = new Message(sessionId, sender, tokens, text, vector, rating);
        }
    }
}
