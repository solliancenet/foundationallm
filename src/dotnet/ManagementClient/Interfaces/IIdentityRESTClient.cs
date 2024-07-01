using FoundationaLLM.Common.Models.Authentication;
using Microsoft.Graph.Models;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Management API's identity endpoints.
    /// </summary>
    public interface IIdentityRESTClient
    {
        /// <summary>
        /// Retrieves a list of group accounts with filtering and paging options.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<Group>> RetrieveGroupsAsync(ObjectQueryParameters parameters);
        /// <summary>
        /// Retrieves a specific group account by its identifier.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<Group> GetGroupAsync(string groupId);
        /// <summary>
        /// Retrieves a list of user accounts with filtering and paging options.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<User>> RetrieveUsersAsync(ObjectQueryParameters parameters);
        /// <summary>
        /// Retrieves a specific user account by its identifier.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<User> GetUserAsync(string userId);
        /// <summary>
        /// Retrieves user and group objects by the passed in list of IDs.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<IEnumerable<DirectoryObject>> RetrieveObjectsByIdsAsync(ObjectQueryParameters parameters);
    }
}
