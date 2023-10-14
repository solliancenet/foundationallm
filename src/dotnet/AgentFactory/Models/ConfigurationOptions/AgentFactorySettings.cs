using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions
{
    public record AgentFactorySettings
    {
        public string DefaultOrchestrationService { init; get; }
    }
}
