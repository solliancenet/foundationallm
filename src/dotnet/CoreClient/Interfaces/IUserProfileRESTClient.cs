using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's user profile endpoints.
    /// </summary>
    public interface IUserProfileRESTClient
    {
        /// <summary>
        /// Retrieves user profiles.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserProfile>> GetUserProfilesAsync();
    }
}
