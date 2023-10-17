using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class OrchestrationRequestTests
    {
        [Fact]
        public void OrchestrationRequest_UserPrompt_SetCorrectly()
        {
            // Arrange
            var orchestrationRequest = new OrchestrationRequest();
            var testUserPrompt = "User_Prompt";

            // Act
            orchestrationRequest.UserPrompt = testUserPrompt;

            // Assert
            Assert.Equal(testUserPrompt, orchestrationRequest.UserPrompt);
        }
    }
}
