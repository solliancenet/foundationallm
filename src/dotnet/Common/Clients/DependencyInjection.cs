using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable IDE0130 // Use of namespace is intentional to support multiple partial classes for dependency injection extension methods.
namespace FoundationaLLM
#pragma warning restore IDE0130 // Use of namespace is intentional to support multiple partial classes for dependency injection extension methods.
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds the authorization service to the dependency injection container (used by all resource providers).
        /// </summary>
        /// <param name="builder"></param>
        public static void AddAuthorizationServiceClient(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<AuthorizationServiceClientSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials));
            builder.Services.AddSingleton<IAuthorizationServiceClient, AuthorizationServiceClient>();
        }

        /// <summary>
        /// Adds the authorization service to the dependency injection container (used by all resource providers).
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddAuthorizationServiceClient(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<AuthorizationServiceClientSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials));
            services.AddSingleton<IAuthorizationServiceClient, AuthorizationServiceClient>();
        }
    }
}
