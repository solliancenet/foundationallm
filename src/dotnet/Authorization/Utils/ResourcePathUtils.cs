using FoundationaLLM.Authorization.ResourceProviders;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Configuration.Services;
using System.Collections.Immutable;

namespace FoundationaLLM.Authorization.Utils
{
    /// <summary>
    /// Utilities dedicated to the <see cref="ResourcePath"/> class.
    /// </summary>
    public static class ResourcePathUtils
    {
        /// <summary>
        /// Parses a resource path from an <see cref="ActionAuthorizationRequest"/> object.
        /// </summary>
        /// <param name="resourcePath">The resource path to be analyzed.</param>
        /// <param name="allowedInstanceIds">The list of allowed FoundationaLLM instance identifiers.</param>
        /// <returns>A <see cref="ResourcePath"/> instance.</returns>
        /// <exception cref="AuthorizationException">Thrown if the resource path is invalid.</exception>
        public static ResourcePath ParseForAuthorizationRequestResourcePath(string resourcePath, List<string> allowedInstanceIds) =>
            ParseInternal(resourcePath, allowedInstanceIds, false);

        public static ResourcePath ParseForRoleAssignmentScope(string resourcePath, List<string> allowedInstanceIds) =>
            ParseInternal(resourcePath, allowedInstanceIds, true);

        private static ResourcePath ParseInternal(
            string resourcePath,
            List<string> allowedInstanceIds,
            bool allowRootPath)
        {
            // Confirm that the resource path has a valid FoundationaLLM instance id.
            if (!ResourcePath.TryParseInstanceId(resourcePath, out var instanceId)
                || !allowedInstanceIds.Contains(instanceId!))
                throw new AuthorizationException("The resource path does not contain a valid FoundationaLLM instance identifier.");

            var parsedResourcePath = GetResourcePath(resourcePath);

            if (!allowRootPath && parsedResourcePath.IsRootPath)
                throw new AuthorizationException("A root resource path is not allowed in this context.");

            return parsedResourcePath;
        }

        private static ResourcePath GetResourcePath(string resourcePath)
        {
            ResourcePath.TryParseResourceProvider(resourcePath, out var resourceProvider);

            var allowedResourceProviders = ImmutableList<string>.Empty;
            var allowedResourceTypes = new Dictionary<string, ResourceTypeDescriptor>();

            if (resourceProvider != null)
            {
                allowedResourceProviders = allowedResourceProviders.Add(resourceProvider);
                allowedResourceTypes = GetAllowedResourceTypes(resourceProvider);
            }

            if (!ResourcePath.TryParse(
                resourcePath,
                allowedResourceProviders,
                allowedResourceTypes,
                false,
                out ResourcePath? parsedResourcePath))
                throw new AuthorizationException($"The resource path [{resourcePath}] is invalid.");

            return parsedResourcePath!;
        }

        private static Dictionary<string, ResourceTypeDescriptor> GetAllowedResourceTypes(string resourceProvider) =>
            resourceProvider switch
            {
                ResourceProviderNames.FoundationaLLM_Agent => AgentResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_DataSource => DataSourceResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Prompt => PromptResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Vectorization => VectorizationResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Configuration => ConfigurationResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Attachment => AttachmentResourceProviderMetadata.AllowedResourceTypes,
                ResourceProviderNames.FoundationaLLM_Authorization => AuthorizationResourceProviderMetadata.AllowedResourceTypes,
                _ => []
            };
    }
}
