using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Chat
{
    public class MessageHistory
    {
        public string Sender { get; set; }
        public string Text { get; set; }

        public MessageHistory(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }
    }
}
