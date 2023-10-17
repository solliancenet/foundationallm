using FoundationaLLM.Common.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Configuration
{
    public class KeyVaultConfigurationServiceSettingsTests
    {
        [Fact]
        public void KeyVaultConfigurationServiceSettings_KeyVaultUri_SetCorrectly()
        {
            // Arrange
            var keyVaultSettings = new KeyVaultConfigurationServiceSettings { KeyVaultUri = "KEYVAULT_URL"};

            // Assert
            Assert.Equal("KEYVAULT_URL", keyVaultSettings.KeyVaultUri);
        }
    }
}
