using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Utils
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

        /// <summary>
        /// Parses a resource path used as scope for an RBAC role assignment.
        /// </summary>
        /// <param name="resourcePath">The resource path to be parsed.</param>
        /// <param name="allowedInstanceIds">The list of FoundationaLLM instance identifiers used as the context for parsing.</param>
        /// <returns>A <see cref="ResourcePath"/> object containing the parsed resource path.</returns>
        /// <exception cref="AuthorizationException"></exception>
        /// <remarks>
        /// <para>Technically, the resource path can be any valid resource path, including a root path.
        /// At the moment, role assignments are not allowed at the root level, so the method will throw an exception if the resource path is a root path.
        /// Role assignments at the root level are reserved for future use (e.g., in SaaS scenarios where cross-FoundationaLLM instance assignments are needed).
        /// </para>
        /// <para>
        /// Any non-root resource path must reference one of the allowed FoundationaLLM instance identifiers.
        /// </para>
        /// </remarks>
        public static ResourcePath ParseForRoleAssignmentScope(string resourcePath, List<string> allowedInstanceIds)
        {
            var parsedResourcePath = ParseInternal(resourcePath, allowedInstanceIds, true);

            if (parsedResourcePath.IsRootPath)
                throw new AuthorizationException($"The resource path {resourcePath} cannot be used as a scope for a role assignment.");

            return parsedResourcePath;
        }

        /// <summary>
        /// Parses a resource path used as scope for a PBAC policy assignment.
        /// </summary>
        /// <param name="resourcePath">The resource path to be parsed.</param>
        /// <param name="allowedInstanceIds">The list of FoundationaLLM instance identifiers used as the context for parsing.</param>
        /// <returns>A <see cref="ResourcePath"/> object containing the parsed resource path.</returns>
        /// <exception cref="AuthorizationException"></exception>
        /// <remarks>
        /// <para>
        /// Resource paths used as scopes for policy assignments must be valid resource type paths.
        /// If the resource path is not a valid resource type path, the method will throw an exception.
        /// </para>
        /// <para>
        /// Also, all resource paths must reference one of the allowed FoundationaLLM instance identifiers.
        /// </para>
        /// </remarks>
        public static ResourcePath ParseForPolicyAssignmentScope(string resourcePath, List<string> allowedInstanceIds)
        {
            var parsedResourcePath = ParseInternal(resourcePath, allowedInstanceIds, false);

            if (!parsedResourcePath.IsResourceTypePath)
                throw new AuthorizationException($"The resource path {resourcePath} cannot be used as a scope for a policy assignment.");

            return parsedResourcePath;
        }

        private static ResourcePath ParseInternal(
            string resourcePath,
            List<string> allowedInstanceIds,
            bool allowRootPath)
        {
            // Confirm that the resource path has a valid FoundationaLLM instance id.
            if (!ResourcePath.TryParseInstanceId(resourcePath, out var instanceId)
                || !allowedInstanceIds.Contains(instanceId!))
                throw new AuthorizationException("The resource path does not contain a valid FoundationaLLM instance identifier.");

            var parsedResourcePath = ResourcePath.GetResourcePath(resourcePath);

            if (!allowRootPath && parsedResourcePath.IsRootPath)
                throw new AuthorizationException("A root resource path is not allowed in this context.");

            return parsedResourcePath;
        }
    }
}
