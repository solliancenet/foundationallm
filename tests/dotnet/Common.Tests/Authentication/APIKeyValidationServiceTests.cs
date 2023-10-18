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
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(options);
            var apiKey = "ValidKey";

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void APIKeyValidationService_IsValid_WithInvalidKey_ReturnsFalse()
        {
            // Arrange
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(options);
            var apiKey = "InvalidKey";

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void APIKeyValidationService_IsValid_WithEmptyKey_ReturnsFalse()
        {
            // Arrange
            var options = Options.Create(new APIKeyValidationSettings { APIKey = "ValidKey" });
            var apiKeyValidationService = new APIKeyValidationService(options);
            var apiKey = "";

            // Act
            var isValid = apiKeyValidationService.IsValid(apiKey);

            // Assert
            Assert.False(isValid);
        }
    }
}
