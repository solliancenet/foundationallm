using AzureAI.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the FoundationaLLM.AzureAI resource provider as a singleton service.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <remarks>
        /// Requires an <see cref="GatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureAIResourceProvider(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureAIResourceProvider(builder.Configuration);

        /// <summary>
        /// Registers the FoundationaLLM.AzureAI resource provider as a singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationRoot"/> configuration manager.</param>
        /// <remarks>
        /// Requires an <see cref="IGatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureAIResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {           
            services.AddSingleton<IResourceProviderService, AzureAIResourceProviderService>(sp =>
                new AzureAIResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),                  
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILogger<AzureAIResourceProviderService>>()));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
