using Azure.Core;
using FoundationaLLM.Client.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Core Client service dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Core Client and its related dependencies to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> this method extends to add the Core Client.</param>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public static void AddCoreClient(
            this IServiceCollection services,
            string coreUri,
            TokenCredential credential,
            APIClientSettings? options = null)
        {
            options ??= new APIClientSettings();

            services.AddSingleton<ICoreRESTClient>(serviceProvider => new CoreRESTClient(coreUri, credential, options));
            services.AddSingleton<ICoreClient>(serviceProvider => new CoreClient(coreUri, credential, options));
        }
    }
}
