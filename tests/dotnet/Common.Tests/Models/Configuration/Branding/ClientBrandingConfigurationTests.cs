using FoundationaLLM.Common.Models.Configuration.Branding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Branding
{
    public class ClientBrandingConfigurationTests
    {
        [Fact]
        public void ClientBrandingConfiguration_Properties_SetCorrectly()
        {
            // Arrange
            var brandingConfig = new ClientBrandingConfiguration
            {
                CompanyName = "Company_1",
                FavIconUrl = "Favicon_1",
                LogoUrl = "Logo_1",
                LogoText = "Logo_Text_1",
                PrimaryColor = "#FFFFFF",
                SecondaryColor = "#000000",
                AccentColor = "#FFA500",
                BackgroundColor = "#F0F0F0"
            };

            // Assert
            Assert.Equal("Company_1", brandingConfig.CompanyName);
            Assert.Equal("Favicon_1", brandingConfig.FavIconUrl);
            Assert.Equal("Logo_1", brandingConfig.LogoUrl);
            Assert.Equal("Logo_Text_1", brandingConfig.LogoText);
            Assert.Equal("#FFFFFF", brandingConfig.PrimaryColor);
            Assert.Equal("#000000", brandingConfig.SecondaryColor);
            Assert.Equal("#FFA500", brandingConfig.AccentColor);
            Assert.Equal("#F0F0F0", brandingConfig.BackgroundColor);
        }
    }
}
