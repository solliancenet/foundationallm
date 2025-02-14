using FluentValidation;
using FoundationaLLM.Agent.ResourceProviders;
using FoundationaLLM.Agent.Validation.Metadata;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Agent resource provider service implementation of resource provider dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Agent resource provider and its related services the the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddAgentResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.AddAgentResourceProviderStorage();

            // Register validators.
            builder.Services.AddSingleton<IValidator<AgentBase>, AgentBaseValidator>();
            builder.Services.AddSingleton<IValidator<KnowledgeManagementAgent>, KnowledgeManagementAgentValidator>();
            builder.Services.AddSingleton<IValidator<Workflow>, WorkflowValidator>();
            builder.Services.AddSingleton<IValidator<Tool>, ToolValidator>();

            builder.Services.AddSingleton<IResourceProviderService, AgentResourceProviderService>(sp =>
                new AgentResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp.GetRequiredService<IAzureCosmosDBService>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
