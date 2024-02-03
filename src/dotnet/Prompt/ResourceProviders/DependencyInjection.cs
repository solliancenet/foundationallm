using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Prompt.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register the handler as a hosted service, passing the step name to the handler ctor
        /// </summary>
        /// <param name="services">Application builder service collection</param>
        public static void AddPromptResourceProvider(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                    DependencyInjectionKeys.FoundationaLLM_Prompt_ResourceProviderService)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Prompt_ResourceProviderService_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_Prompt_ResourceProviderService);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_Prompt_ResourceProviderService
                };
            });

            services.AddSingleton<IResourceProviderService, PromptResourceProviderService>(sp =>
                new PromptResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_Prompt_ResourceProviderService),
                    sp.GetRequiredService<ILogger<PromptResourceProviderService>>()));
        }
    }
}
