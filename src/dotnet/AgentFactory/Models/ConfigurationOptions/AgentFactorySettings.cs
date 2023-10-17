using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Settings for an Agent Factory.  Currenlty only sets the default orchestration (Semantickernal, LangChain)
    /// </summary>
    public record AgentFactorySettings
    {
        /// <summary>
        /// The default orchenstration service (SemanticKernal, LangChain)
        /// </summary>
        public string? DefaultOrchestrationService { init; get; }
    }
}
