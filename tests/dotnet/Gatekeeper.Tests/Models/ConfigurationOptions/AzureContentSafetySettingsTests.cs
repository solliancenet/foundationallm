using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Tests.Models.ConfigurationOptions
{
    public class AzureContentSafetySettingsTests
    {
        [Fact]
        public void AzureContentSafetySettings_Properties_SetCorrectly()
        {
            // Arrange
            var azureContentSafetySettings = new AzureContentSafetySettings
            {
                APIUrl = "Endpoint_1",
                APIKey = "API-KEY-SECRET",
                HateSeverity = 1,
                ViolenceSeverity = 2,
                SelfHarmSeverity = 3,
                SexualSeverity = 4
            };

            // Assert
            Assert.Equal("Endpoint_1", azureContentSafetySettings.APIUrl);
            Assert.Equal("API-KEY-SECRET", azureContentSafetySettings.APIKey);
            Assert.Equal(1, azureContentSafetySettings.HateSeverity);
            Assert.Equal(2, azureContentSafetySettings.ViolenceSeverity);
            Assert.Equal(3, azureContentSafetySettings.SelfHarmSeverity);
            Assert.Equal(4, azureContentSafetySettings.SexualSeverity);
        }
    }
}
