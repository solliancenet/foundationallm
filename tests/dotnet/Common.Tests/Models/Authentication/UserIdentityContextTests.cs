using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Authentication
{
    public class UserIdentityContextTests
    {
        [Fact]
        public void UserIdentityContext_CurrentUserIdentity_SetCorrectly()
        {
            // Arrange
            var userIdentityContext = new UserIdentityContext();
            var testUnifiedUserIdentity = new UnifiedUserIdentity
            {
                Name = "Name_1",
                Username = "Username_1",
                UPN = "UPN_1"
            };

            // Act
            userIdentityContext.CurrentUserIdentity = testUnifiedUserIdentity;

            // Assert
            Assert.Equal(testUnifiedUserIdentity, userIdentityContext.CurrentUserIdentity);
        }
    }
}
