using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Tests.Models.ConfigurationOptions
{
    public class RefinementServiceSettingsTests
    {
        [Fact]
        public void RefinementServiceSettings_IsConstructedCorrectly()
        {
            // Arrange
            var refinementServiceSettings = new RefinementServiceSettings();

            // Assert
            Assert.NotNull(refinementServiceSettings);
        }
    }
}
