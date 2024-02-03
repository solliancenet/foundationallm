using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register instance settings mapped to configuration.
        /// </summary>
        /// <param name="services">Application builder service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> providing access to configuration.</param>
        public static void AddInstanceProperties(this IServiceCollection services, IConfigurationManager configuration) =>
            services.AddOptions<InstanceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
    }
}
