using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubResponse
    {
            public string? name { get; set; }
            public string? description { get; set; }

            public List<string> allowed_data_source_names { get; set; }
    }
}
