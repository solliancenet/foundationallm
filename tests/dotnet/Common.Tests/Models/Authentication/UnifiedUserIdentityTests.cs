using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Authentication
{
    public class UnifiedUserIdentityTests
    {
        [Fact]
        public void UnifiedUserIdentity_SetProperties_ReturnsCorrectValues()
        {
            // Arrange
            var unifiedUserIdentity = new UnifiedUserIdentity();
            var testName = "Name_1";
            var testUsername = "Username_1";
            var testUPN = "UPN_1";

            // Act
            unifiedUserIdentity.Name = testName;
            unifiedUserIdentity.Username = testUsername;
            unifiedUserIdentity.UPN = testUPN;

            // Assert
            Assert.Equal(testName, unifiedUserIdentity.Name);
            Assert.Equal(testUsername, unifiedUserIdentity.Username);
            Assert.Equal(testUPN, unifiedUserIdentity.UPN);
        }
    }
}
