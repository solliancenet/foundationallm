using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Service that provides a common interface for creating <see cref="HttpClient"/>
    /// instances from <see cref="IHttpClientFactory"/>. and ensures that all
    /// necessary headers are added to the request.
    /// </summary>
    public interface IHttpClientFactoryService
    {
        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance from the provided client name.
        /// The client name must be registered in the <see cref="IHttpClientFactory"/>
        /// configuration.
        /// The headers added to the request are:
        /// - X-API-KEY: The API key for the target API.
        /// - X-USER-IDENTITY: The user identity information for the current user.
        /// </summary>
        /// <param name="clientName">The named <see cref="HttpClient"/> client configuration.</param>
        /// <returns></returns>
        HttpClient CreateClient(string clientName);
    }
}
