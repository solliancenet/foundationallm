using FoundationaLLM.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class DownstreamAPIKeySettingsTests
    {
        [Fact]
        public void DownstreamAPIKeySettings_Properties_SetCorrectly()
        {
            // Arrange
            var downstreamApiKeySettings = new DownstreamAPIKeySettings
            {
                APIUrl = "URL_1",
                APIKey = "API_KEY_SECRET"
            };

            // Assert
            Assert.Equal("URL_1", downstreamApiKeySettings.APIUrl);
            Assert.Equal("API_KEY_SECRET", downstreamApiKeySettings.APIKey);
        }

        [Fact]
        public void DownstreamAPISettings_DownstreamAPIs_SetCorrectly()
        {
            // Arrange
            var downstreamAPIs = new Dictionary<string, DownstreamAPIKeySettings>
            {
                { "API_1", new DownstreamAPIKeySettings { APIUrl = "URL_1", APIKey = "API_KEY_SECRET" } }
            };

            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = downstreamAPIs
            };

            // Assert
            Assert.Equal(downstreamAPIs, downstreamAPISettings.DownstreamAPIs);
        }
    }
}
