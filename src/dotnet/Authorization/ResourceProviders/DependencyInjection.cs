using FluentValidation;
using FoundationaLLM.Authorization.ResourceProviders;
using FoundationaLLM.Authorization.Validation;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
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
        /// Register the FoundationaLLM.Authorization resource provider with the dependency injection container.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        public static void AddAuthorizationResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IValidator<RoleAssignment>, RoleAssignmentValidator>();

            builder.Services.AddSingleton<IResourceProviderService, AuthorizationResourceProviderService>(sp =>
                new AuthorizationResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
