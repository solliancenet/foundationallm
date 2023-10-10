using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration
{
    public record KeyVaultConfigurationServiceSettings
    {
        public required string KeyVaultUri { get; set; }
    }
}
