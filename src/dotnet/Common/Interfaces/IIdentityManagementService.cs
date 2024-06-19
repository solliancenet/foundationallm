using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Collections;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides identity management services.
    /// </summary>
    public interface IIdentityManagementService
    {
        /// <summary>
        /// Retrieves the group identifiers list of the groups where the specified user principal is a member.
        /// </summary>
        /// <param name="userIdentifier">The user identifier for which group membership is retrieved. Can be either an object id or a user principal name (UPN).</param>
        /// <returns></returns>
        Task<List<string>> GetGroupsForPrincipal(string userIdentifier);

        /// <summary>
        /// Retrieves user and group objects by the passed in list of IDs.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<List<ObjectQueryResult>> GetObjectsByIds(ObjectQueryParameters parameters);

        /// <summary>
        /// Retrieves a list of group accounts with filtering and paging options.
        /// </summary>
        /// <param name="queryParams">The filtering and paging options used when retrieving group accounts.</param>
        /// <returns></returns>
        Task<PagedResponse<ObjectQueryResult>> GetUserGroups(ObjectQueryParameters queryParams);

        /// <summary>
        /// Retrieves a group account by its identifier.
        /// </summary>
        /// <param name="groupId">The group account identifier used to retrieve a single group account.</param>
        /// <returns></returns>
        Task<ObjectQueryResult> GetUserGroupById(string groupId);

        /// <summary>
        /// Retrieves a list of user accounts with filtering and paging options.
        /// </summary>
        /// <param name="queryParams">The filtering and paging options used when retrieving users.</param>
        /// <returns></returns>
        Task<PagedResponse<ObjectQueryResult>> GetUsers(ObjectQueryParameters queryParams);

        /// <summary>
        /// Retrieves a user account by its identifier.
        /// </summary>
        /// <param name="userId">The user identifier used to retrieve a single user account.</param>
        /// <returns></returns>
        Task<ObjectQueryResult> GetUserById(string userId);
    }
}
