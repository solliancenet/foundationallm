using FluentValidation;
using FoundationaLLM.Authorization.Interfaces;
using FoundationaLLM.Authorization.Models.Configuration;
using FoundationaLLM.Authorization.Services;
using FoundationaLLM.Authorization.Validation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
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
        /// Add the Authorization Core service to the dependency injection container (used by the Authorization API).
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddAuthorizationCore(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IStorageService, DataLakeStorageService>(sp =>
            {
                return new DataLakeStorageService(
                    Options.Create<BlobStorageServiceSettings>(new BlobStorageServiceSettings
                    {
                        AuthenticationType = BlobStorageAuthenticationTypes.AzureIdentity,
                        AccountName = builder.Configuration[KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_Storage_AccountName]
                    }),
                    sp.GetRequiredService<ILogger<DataLakeStorageService>>())
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Authorization
                };
            });

            // Register validators.
            builder.Services.AddSingleton<IValidator<ActionAuthorizationRequest>, ActionAuthorizationRequestValidator>();

            builder.Services.AddSingleton<IAuthorizationCore, AuthorizationCore>(sp => new AuthorizationCore(
                    Options.Create<AuthorizationCoreSettings>(new AuthorizationCoreSettings
                    {
                        InstanceIds = [.. builder.Configuration[KeyVaultSecretNames.FoundationaLLM_AuthorizationAPI_InstanceIds]!.Split(',')]
                    }),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Authorization),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<ILogger<AuthorizationCore>>()));

            builder.Services.ActivateSingleton<IAuthorizationCore>();
        }

        /// <summary>
        /// Add the authorization service to the dependency injection container (used by all resource providers).
        /// </summary>
        /// <param name="builder"></param>
        public static void AddAuthorizationService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<AuthorizationServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_AuthorizationAPI));
            builder.Services.AddSingleton<IAuthorizationService, AuthorizationService>();
        }
    }
}
