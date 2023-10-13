using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class SummaryResponseTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedInfo = "Info_1";

            // Act
            var summaryResponse = new SummaryResponse
            {
                Info = expectedInfo
            };

            // Assert
            Assert.Equal(expectedInfo, summaryResponse.Info);
        }

    }
}
