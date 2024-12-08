using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.ResourceProviders;
using FoundationaLLM.Vectorization.Validation.Resources;
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
            builder.AddVectorizationResourceProviderStorage();         

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
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),                    
                    sp,                    
                    sp.GetRequiredService<ILoggerFactory>()));          

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
