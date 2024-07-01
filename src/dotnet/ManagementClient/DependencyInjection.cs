using Azure.Core;
using FoundationaLLM.Client.Management;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM
{
    /// <summary>
    /// Management Client service dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Management Client and its related dependencies to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> this method extends to add the Core Client.</param>
        /// <param name="managementUri">The base URI of the Management API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public static void AddManagementClient(
            this IServiceCollection services,
            string managementUri,
            TokenCredential credential,
            string instanceId,
            APIClientSettings? options = null)
        {
            options ??= new APIClientSettings();

            services.AddSingleton<IManagementRESTClient>(serviceProvider => new ManagementRESTClient(managementUri, credential, instanceId, options));
            services.AddSingleton<IManagementClient>(serviceProvider => new ManagementClient(managementUri, credential, instanceId, options));
        }
    }
}
