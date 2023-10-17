using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class APIKeyValidationServiceTests
    {
        [Fact]
        public void APIKeyValidationService_IsValid_WithValidKey_ReturnsTrue()
        {
            // Arrange
            var configurationService = Substitute.For<IConfigurationService>();
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(configurationService, options);
            var apiKey = "ValidKey";

            configurationService.GetValue<string>("ValidKey").Returns(apiKey);

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void APIKeyValidationService_IsValid_WithInvalidKey_ReturnsFalse()
        {
            // Arrange
            var configurationService = Substitute.For<IConfigurationService>();
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(configurationService, options);
            var apiKey = "InvalidKey";

            configurationService.GetValue<string>("ValidKey").Returns("ValidKey");

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void APIKeyValidationService_IsValid_WithEmptyKey_ReturnsFalse()
        {
            // Arrange
            var configurationService = Substitute.For<IConfigurationService>();
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(configurationService, options);
            var apiKey = "";

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.False(isValid);
        }
    }
}
