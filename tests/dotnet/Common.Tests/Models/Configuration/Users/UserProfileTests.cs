using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Users
{
    public class UserProfileTests
    {
        public UserProfile _userProfile = new UserProfile("test@domain.com", new List<Agent>
        {
            new Agent { Name = "Agent1" },
            new Agent { Name = "Agent2" }
        });

        [Fact]
        public void UserProfile_DefaultConstructor_SetsPropertiesCorrectly()
        {
            // Act and Assert
            Assert.Equal("test@domain.com", _userProfile.Id);
            Assert.Equal("UserProfile", _userProfile.Type);
            Assert.Equal("test@domain.com", _userProfile.UPN);
            Assert.Equal(2, _userProfile.PrivateAgents?.Count());
        }

        [Fact]
        public void UserProfile_JsonSerialization_DeserializesCorrectly()
        {
            // Act
            var json = JsonConvert.SerializeObject(_userProfile);
            var deserializedProfile = JsonConvert.DeserializeObject<UserProfile>(json);

            // Assert
            Assert.Equivalent(deserializedProfile, _userProfile);
        }
    }
}
