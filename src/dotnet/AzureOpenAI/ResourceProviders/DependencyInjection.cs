﻿using FoundationaLLM.AzureOpenAI.ResourceProviders;
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
        /// Requires a <see cref="GatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureOpenAIResourceProvider(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureOpenAIResourceProvider(builder.Configuration);

        /// <summary>
        /// Registers the FoundationaLLM.AzureOpenAI resource provider as a singleton service.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationRoot"/> configuration manager.</param>
        /// <remarks>
        /// Requires a <see cref="GatewayServiceClient"/> service to be also registered with the dependency injection container.
        /// </remarks>
        public static void AddAzureOpenAIResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddSingleton<IResourceProviderService, AzureOpenAIResourceProviderService>(sp =>
                new AzureOpenAIResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),                    
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILogger<AzureOpenAIResourceProviderService>>()));

            services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
