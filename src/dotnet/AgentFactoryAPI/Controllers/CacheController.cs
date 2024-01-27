using Asp.Versioning;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    /// <summary>
    /// Provides methods for managing the agents artifacts cache.
    /// </summary>
    /// <param name="cacheService">The <see cref="ICacheService"/> cache holding the agents artifacts.</param>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class CacheController(
        ICacheService cacheService) : ControllerBase
    {
        private readonly ICacheService _cacheService = cacheService;

        /// <summary>
        /// Removes an object from the cache by its name.
        /// </summary>
        /// <param name="name">The name of the object to be removed from the cache.</param>
        [HttpPost("item/{name}/remove")]
        public IActionResult Remove(string name)
        {
            _cacheService.Remove(new CacheKey(name, string.Empty));
            return Ok();
        }

        /// <summary>
        /// Removes all objects belonging to a category from the cache.
        /// </summary>
        /// <param name="name">The name of the category of objects to be removed from the cache.</param>
        [HttpPost("category/{name}/remove")]
        public IActionResult RemoveCategory(string name)
        {
            _cacheService.RemoveByCategory(new CacheKey(string.Empty, name));
            return Ok();
        }
    }
}
