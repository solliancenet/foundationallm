using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Prompt.ResourceProviders;
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
        /// Register the handler as a hosted service, passing the step name to the handler ctor
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddPromptResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddPromptResourceProviderStorage();

            builder.Services.AddSingleton<IResourceProviderService, PromptResourceProviderService>(sp =>
                new PromptResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILogger<PromptResourceProviderService>>()));

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
