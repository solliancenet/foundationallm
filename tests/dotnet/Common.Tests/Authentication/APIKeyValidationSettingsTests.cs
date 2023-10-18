using FoundationaLLM.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class APIKeyValidationSettingsTests
    {
        [Fact]
        public void APIKeyValidationSettings_APIKeySecretName_SetCorrectly()
        {
            // Arrange
            var settings = new APIKeyValidationSettings
            {
                APIKey = "API_KEY_SECRET"
            };

            // Assert
            Assert.Equal("API_KEY_SECRET", settings.APIKey);
        }
    }
}
