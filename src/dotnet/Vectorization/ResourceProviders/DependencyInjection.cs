using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// Register the handler as a hosted service, passing the step name to the handler ctor
        /// </summary>
        /// <param name="services">Application builder service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> providing access to configuration.</param>
        public static void AddVectorizationResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_ResourceProviderService_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService
                };
            });

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            services.AddSingleton<IResourceProviderService, VectorizationResourceProviderService>(sp =>
                new VectorizationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService),
                    sp.GetRequiredService<ILogger<VectorizationResourceProviderService>>()));
        }
    }
}
