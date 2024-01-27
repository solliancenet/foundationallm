using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Exposes functionality for calling the Agent Factory API.
    /// </summary>
    public interface IAgentFactoryAPIService
    {
        /// <summary>
        /// Removes a specific cache item by its name within the Agent Factory API.
        /// </summary>
        /// <param name="name">The name of the object to be removed from the cache.</param>
        Task<bool> RemoveCacheItem(string name);

        /// <summary>
        /// Removes all objects belonging to a category from the cache within the Agent Factory API.
        /// </summary>
        /// <param name="name">The name of the category of objects to be removed from the cache.</param>
        Task<bool> RemoveCacheCategory(string name);
    }
}
