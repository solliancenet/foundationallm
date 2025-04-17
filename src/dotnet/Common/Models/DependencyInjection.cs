using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers an instance of <see cref="DependencyInjectionContainerSettings"/> with default settings to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="settings">An optional <see cref="DependencyInjectionContainerSettings"/> instance that overrides the default settings.</param>
        public static void AddDIContainerSettings(
            this IHostApplicationBuilder builder,
            DependencyInjectionContainerSettings? settings = null) =>
            builder.Services.AddDIContainerSettings(settings);

        /// <summary>
        /// Registers an instance of <see cref="DependencyInjectionContainerSettings"/> with default settings to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="settings">An optional <see cref="DependencyInjectionContainerSettings"/> instance that overrides the default settings.</param>
        public static void AddDIContainerSettings(
            this IServiceCollection services,
            DependencyInjectionContainerSettings? settings = null) =>
            services.AddSingleton<DependencyInjectionContainerSettings>(
                settings ?? new DependencyInjectionContainerSettings());

        /// <summary>
        /// Registers the <see cref="IOrchestrationContext"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddOrchestrationContext(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddOrchestrationContext();

        /// <summary>
        /// Registers the <see cref="IOrchestrationContext"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddOrchestrationContext(
            this IServiceCollection services) =>
            services.AddScoped<IOrchestrationContext, OrchestrationContext>();
    }
}
