using FoundationaLLM.Common.Settings;
using Polly;

namespace FoundationaLLM.Common.Tests.Settings
{
    public class CommonHttpRetryStrategyOptionsTests
    {
        [Fact]
        public void GetCommonHttpRetryStrategyOptions_ReturnsCorrectOptions()
        {
            // Act
            var options = CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();

            // Assert
            Assert.NotNull(options);
            Assert.Equal(DelayBackoffType.Exponential, options.BackoffType);
            Assert.Equal(5, options.MaxRetryAttempts);
            Assert.True(options.UseJitter);
        }
    }
}
