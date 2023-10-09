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
        public static Message CreateMessage()
        {
            return new Message("1", "sender1", 0, "New message", null, null);
        }
    }
}
