using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceProviderActionResultTests
    {
        [Fact]
        public void ResourceProviderActionResult_IsSuccessResult_True_Test()
        {
            // Arrange
            bool isSuccess = true;
            string objectId = string.Empty;

            // Act
            var result = new ResourceProviderActionResult(objectId, isSuccess);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
