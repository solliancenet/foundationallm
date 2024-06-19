using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

namespace FoundationaLLM
{
    /// <summary>
    /// Dependency injection services for security services.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add group membership services to dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddGroupMembership(this IHostApplicationBuilder builder)
        {
            // Register the Microsoft Graph API client.
            builder.Services.AddSingleton(provider => new GraphServiceClient(DefaultAuthentication.AzureCredential));

            // Register the group membership service.
            builder.Services.AddSingleton<IIdentityManagementService, MicrosoftGraphIdentityManagementService>();
        }
    }
}
