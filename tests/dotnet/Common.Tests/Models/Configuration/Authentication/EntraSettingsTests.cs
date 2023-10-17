using FoundationaLLM.Common.Models.Configuration.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Authentication
{
    public class EntraSettingsTests
    {
        [Fact]
        public void EntraSettings_Scopes_SetCorrectly()
        {
            // Arrange
            var entraSettings = new EntraSettings();
            var testScopes = "Scope_1";

            // Act
            entraSettings.Scopes = testScopes;

            // Assert
            Assert.Equal(testScopes, entraSettings.Scopes);
        }
    }
}
