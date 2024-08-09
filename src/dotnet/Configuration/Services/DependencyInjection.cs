using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Configuration.Services;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register the FoundationaLLM.Configuration resource provider with the dependency injection container.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        public static void AddConfigurationResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddAzureKeyVaultService(AppConfigurationKeys.FoundationaLLM_Configuration_KeyVaultURI);

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddConfigurationClient(
                    builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
            });

            builder.Services.AddSingleton<IAzureAppConfigurationService, AzureAppConfigurationService>();

            builder.AddConfigurationResourceProviderStorage();

            builder.Services.AddSingleton<IResourceProviderService, ConfigurationResourceProviderService>(sp =>
                new ConfigurationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureAppConfigurationService>(),
                    sp.GetRequiredService<IAzureKeyVaultService>(),
                    builder.Configuration,
                    sp,
                    sp.GetRequiredService<ILogger<ConfigurationResourceProviderService>>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
