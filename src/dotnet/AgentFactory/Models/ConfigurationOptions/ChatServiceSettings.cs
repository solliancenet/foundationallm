using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Models.ConfigurationOptions
{
    public record ChatServiceSettings
    {
        public required string DefaultOrchestrationService { get; init; }
    }
}
