using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions
{
    public class SettingsBase
    {
        public string? APIUrl { get; init; }
        public string? APIKey { get; init; }
    }
}
