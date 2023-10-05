using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Chat
{
    /// <summary>
    /// Represents the historic message sender and text.
    /// </summary>
    public class MessageHistoryItem
    {
        /// <summary>
        /// The sender of the message (e.g. "Agent", "User").
        /// </summary>
        public string Sender { get; set; }
        /// <summary>
        /// The message text.
        /// </summary>
        public string Text { get; set; }

        public MessageHistoryItem(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }
    }
}
