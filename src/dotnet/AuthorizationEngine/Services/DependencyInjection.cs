using FluentValidation;
using FoundationaLLM.AuthorizationEngine.Interfaces;
using FoundationaLLM.AuthorizationEngine.Models.Configuration;
using FoundationaLLM.AuthorizationEngine.Services;
using FoundationaLLM.AuthorizationEngine.Validation;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        /// Adds the Authorization Core service to the dependency injection container (used by the Authorization API).
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddAuthorizationCore(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IStorageService, DataLakeStorageService>(sp =>
            {
                return new DataLakeStorageService(
                    Options.Create<BlobStorageServiceSettings>(new BlobStorageServiceSettings
                    {
                        AuthenticationType = AuthenticationTypes.AzureIdentity,
                        AccountName = builder.Configuration[AuthorizationKeyVaultSecretNames.FoundationaLLM_ResourceProviders_Authorization_Storage_AccountName]
                    }),
                    sp.GetRequiredService<ILogger<DataLakeStorageService>>())
                {
                    InstanceName = AuthorizationDependencyInjectionKeys.FoundationaLLM_ResourceProviders_Authorization
                };
            });

            // Register validators.
            builder.Services.AddSingleton<IValidator<ActionAuthorizationRequest>, ActionAuthorizationRequestValidator>();

            builder.Services.AddSingleton<IAuthorizationCore, AuthorizationCore>(sp => new AuthorizationCore(
                    Options.Create<AuthorizationCoreSettings>(new AuthorizationCoreSettings
                    {
                        InstanceIds = [.. builder.Configuration[AuthorizationKeyVaultSecretNames.FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_InstanceIds]!.Split(',')]
                    }),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == AuthorizationDependencyInjectionKeys.FoundationaLLM_ResourceProviders_Authorization),
                    sp.GetRequiredService<IAzureKeyVaultService>(),
                    builder.Configuration,
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<ILogger<AuthorizationCore>>()));

            builder.Services.ActivateSingleton<IAuthorizationCore>();
        }
    }
}
