using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubMessage
    {
        public string? user_prompt { get; set; }
        public string? user_context { get; set; }

    }
}
