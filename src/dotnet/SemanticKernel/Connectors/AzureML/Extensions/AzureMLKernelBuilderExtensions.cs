using FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Extensions
{
    /// <summary>
    /// Extensions for adding AzureML chat completion services.
    /// </summary>
    public static class AzureMLKernelBuilderExtensions
    {

        /// <summary>
        /// Add an AzureML chat completion service to the kernel builder.
        /// </summary>
        /// <param name="builder">The kernel builder.</param>
        /// <param name="endpoint">The endpoint of the AzureML service.</param>
        /// <param name="apiKey">The API key for the AzureML service.</param>
        /// <param name="deploymentName">The optional name of the deployment.</param>
        /// <param name="serviceId">The optional service ID.</param>
        /// <returns>The updated kernel builder.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IKernelBuilder AddAzureMLChatCompletion(
            this IKernelBuilder builder, string endpoint, string apiKey, string? deploymentName = null, string? serviceId = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("The endpoint is required.", nameof(endpoint));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("The API key is required.", nameof(apiKey));

            builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
                new AzureMLChatCompletionService(
                        endpoint,
                        apiKey, 
                        deploymentName,
                        loggerFactory: serviceProvider.GetService<ILoggerFactory>()));

            return builder;
        }

    }
}
