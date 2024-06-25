using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides account retrieval methods.
    /// </summary>
    /// <param name="callContext">The call context containing user identity details.</param>
    /// <param name="identityManagementService">The <see cref="IIdentityManagementService"/> used for retrieving group account information.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/identity")]
    public class IdentityManagementController(
        ICallContext callContext,
        IIdentityManagementService identityManagementService,
        ILogger<IdentityManagementController> logger) : Controller
    {
        private readonly ILogger<IdentityManagementController> _logger = logger;
        private readonly ICallContext _callContext = callContext;

        /// <summary>
        /// Retrieves a list of group accounts with filtering and paging options.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost("groups/retrieve", Name = "RetrieveGroups")]
        public async Task<IActionResult> RetrieveGroups(ObjectQueryParameters parameters)
        {
            var groups = await identityManagementService.GetUserGroups(parameters);
            return new OkObjectResult(groups);
        }

        /// <summary>
        /// Retrieves a specific group account by its identifier.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("groups/{groupId}", Name = "GetGroup")]
        public async Task<IActionResult> GetGroup(string groupId)
        {
            var group = await identityManagementService.GetUserGroupById(groupId);
            return new OkObjectResult(group);
        }

        /// <summary>
        /// Retrieves a list of user accounts with filtering and paging options.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost("users/retrieve", Name = "RetrieveUsers")]
        public async Task<IActionResult> RetrieveUsers(ObjectQueryParameters parameters)
        {
            var users = await identityManagementService.GetUsers(parameters);
            return new OkObjectResult(users);
        }

        /// <summary>
        /// Retrieves a specific user account by its identifier.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("users/{userId}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(string userId)
        {
            var user = await identityManagementService.GetUserById(userId);
            return new OkObjectResult(user);
        }

        /// <summary>
        /// Retrieves user and group objects by the passed in list of IDs.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [HttpPost("objects/retrievebyids", Name = "RetrieveObjectsByIds")]
        public async Task<IActionResult> RetrieveObjectsByIds(ObjectQueryParameters parameters)
        {
            var objects = await identityManagementService.GetObjectsByIds(parameters);
            return new OkObjectResult(objects);
        }
    }
}
