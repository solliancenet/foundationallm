using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Vectorization.ResourceProviders;
using FoundationaLLM.Vectorization.Validation.Resources;
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
        /// Register the handler as a hosted service, passing the step name to the handler ctor.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddVectorizationResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_ResourceProviderService_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization
                };
            });            

            // Register validators.
            builder.Services.AddSingleton<IValidator<TextPartitioningProfile>, TextPartitioningProfileValidator>();
            builder.Services.AddSingleton<IValidator<TextEmbeddingProfile>, TextEmbeddingProfileValidator>();
            builder.Services.AddSingleton<IValidator<IndexingProfile>, IndexingProfileValidator>();
            builder.Services.AddSingleton<IValidator<VectorizationPipeline>, VectorizationPipelineValidator>();
            builder.Services.AddSingleton<IValidator<VectorizationRequest>, VectorizationRequestValidator>();                                   
            
            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            builder.Services.AddSingleton<IResourceProviderService>(sp => 
                new VectorizationResourceProviderService(                   
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),                    
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),                    
                    sp,                    
                    sp.GetRequiredService<ILoggerFactory>()));          

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
