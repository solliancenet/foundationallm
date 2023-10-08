using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    public interface IAuthenticatedHttpClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance from <see cref="IHttpClientFactory"/> with an
        /// authorization header for the current user.
        /// </summary>
        /// <param name="clientName">The named <see cref="HttpClient"/> client configuration.</param>
        /// <param name="scopes">List of permissions to request from the service.</param>
        /// <returns></returns>
        Task<HttpClient> CreateClientAsync(string clientName, string scopes);
    }
}
