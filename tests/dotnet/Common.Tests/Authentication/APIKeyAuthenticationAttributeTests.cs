using FoundationaLLM.Common.Authentication;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class APIKeyAuthenticationAttributeTests
    {
        [Fact]
        public void APIKeyAuthenticationAttribute_Constructor_SetsServiceType()
        {
            // Arrange
            var attribute = new APIKeyAuthenticationAttribute();

            // Act
            var serviceType = attribute.ServiceType;

            // Assert
            Assert.Equal(typeof(APIKeyAuthenticationFilter), serviceType);
        }
    }
}
