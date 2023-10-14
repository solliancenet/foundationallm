using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class SummaryRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedPrompt = "Prompt_1";

            // Act
            var summaryRequest = new SummaryRequest
            {
                Prompt = expectedPrompt
            };

            // Assert
            Assert.Equal(expectedPrompt, summaryRequest.Prompt);
        }
    }
}
