using FoundationaLLM.AzureOpenAI.ResourceProviders;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Configuration;
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
        /// Registers the FoundationaLLM.AzureOpenAI resource provider as a singleton service.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <remarks>
        /// Requires an <see cref="IGatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureOpenAIResourceProvider(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureOpenAIResourceProvider(builder.Configuration);

        /// <summary>
        /// Registers the FoundationaLLM.AzureOpenAI resource provider as a singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationRoot"/> configuration manager.</param>
        /// <remarks>
        /// Requires an <see cref="IGatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureOpenAIResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddAzureOpenAIResourceProviderStorage(configuration);

            services.AddSingleton<IResourceProviderService, AzureOpenAIResourceProviderService>(sp =>
                new AzureOpenAIResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AzureOpenAI),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILogger<AzureOpenAIResourceProviderService>>()));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
