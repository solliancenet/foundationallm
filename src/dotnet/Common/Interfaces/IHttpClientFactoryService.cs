using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

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
        /// Creates a <see cref="HttpClient"/> instance based on the client name.
        /// The client name must be registered in the <see cref="IHttpClientFactory"/> configuration.
        /// </summary>
        /// <param name="clientName">The name of the HTTP client to create. This name must be registered as an <see cref="APIEndpointConfiguration"/> resource in the FoundationaLLM.Configuration resource provider.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        Task<HttpClient> CreateClient(string clientName, UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Creates a <typeparamref name="T"/> client instance based on the client name and a client builder delegate.
        /// </summary>
        /// <typeparam name="T">The type of the client to create.</typeparam>
        /// <param name="clientName">The name of the HTTP client to create. This name must be registered as an <see cref="APIEndpointConfiguration"/> resource in the FoundationaLLM.Configuration resource provider.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <param name="clientBuilder">A delegate that creates the <typeparamref name="T"/> client instance based on a dictionary of values. The keys available in the dictionary are defined in <see cref="HttpClientFactoryServiceKeyNames"/>.</param>
        /// <param name="clientBuilderParameters">A dictionary of parameters to pass to the client builder delegate.</param>
        /// <returns>A <typeparamref name="T"/> client instance.</returns>
        Task<T> CreateClient<T>(
            string clientName,
            UnifiedUserIdentity userIdentity,
            Func<Dictionary<string, object>, T> clientBuilder,
            Dictionary<string, object>? clientBuilderParameters = null);

        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance based on the endpoint configuration.
        /// </summary>
        /// <param name="endpointConfiguration">The <see cref="APIEndpointConfiguration"/> resource used to create the client.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> of the caller requesting the client.</param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        Task<HttpClient> CreateClient(APIEndpointConfiguration endpointConfiguration, UnifiedUserIdentity? userIdentity);

        /// <summary>
        /// Creates a new unregistered <see cref="HttpClient"/> instance with a timeout.
        /// </summary>
        /// <param name="timeout">The timeout for the <see cref="HttpClient"/>.
        /// If not specified, the default timeout in seconds is applied.
        /// For an infinite waiting period, use <see cref="Timeout.InfiniteTimeSpan"/></param>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        HttpClient CreateUnregisteredClient(TimeSpan? timeout = null);
    }
}
