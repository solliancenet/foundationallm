using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides the logic for handling FoundationaLLM resource identifiers.
    /// </summary>
    public class ResourcePath
    {
        private readonly string? _instanceId;
        private readonly string? _resourceProvider;
        private readonly List<ResourceTypeInstance> _resourceTypeInstances;
        private readonly bool _isRootPath;

        private const string INSTANCE_TOKEN = "instances";
        private const string RESOURCE_PROVIDER_TOKEN = "providers";

        /// <summary>
        /// The instance id of the resource identifier. Can be null if the resource path does not contain an instance id.
        /// </summary>
        public string? InstanceId => _instanceId;

        /// <summary>
        /// The resource provider of the resource identifier. Can be null if the resource path does not contain a resource provider.
        /// </summary>
        public string? ResourceProvider => _resourceProvider;

        /// <summary>
        /// The resource type instances of the resource identifier.
        /// </summary>
        public List<ResourceTypeInstance> ResourceTypeInstances => _resourceTypeInstances;

        /// <summary>
        /// Indicates whether the resource path is a root path ("/") or not.
        /// </summary>
        public bool IsRootPath => _isRootPath;

        /// <summary>
        /// Indicates whether the resource path refers to a resource type (does not contain a resource name).
        /// </summary>
        public bool IsResourceTypePath =>
            _resourceTypeInstances != null
            && _resourceTypeInstances.Count > 0
            && _resourceTypeInstances.Last().ResourceId == null;

        /// <summary>
        /// The main resource type of the path.
        /// </summary>
        public string MainResourceType =>
            _resourceTypeInstances == null || _resourceTypeInstances.Count == 0
            ? throw new ResourceProviderException()
            : _resourceTypeInstances[0].ResourceType;

        /// <summary>
        /// Indicates whether the resource path is an instance path or not (i.e., only contains the FoundationaLLM instance identifier).
        /// </summary>
        public bool IsInstancePath =>
            !(_instanceId == null)
            && (_resourceProvider == null);

        /// <summary>
        /// Creates a new resource identifier from a resource path optionally allowing an action.
        /// </summary>
        /// <param name="resourcePath">The resource path used to create the resource identifier.</param>
        /// <param name="allowedResourceProviders">Provides a list of allowed resource providers.</param>
        /// <param name="allowedResourceTypes">Provides a dictionary of <see cref="ResourceTypeDescriptor"/> used to validate resource types.</param>
        /// <param name="allowAction">Optional parameter that indicates whether an action name is allowed as part of the resource identifier.</param>
        public ResourcePath(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction = true) =>
            ParseResourcePath(
                resourcePath,
                allowedResourceProviders,
                allowedResourceTypes,
                allowAction,
                out _isRootPath,
                out _instanceId,
                out _resourceProvider,
                out _resourceTypeInstances);

        /// <summary>
        /// Tries to parse a resource path and create a resource identifier from it.
        /// </summary>
        /// <param name="resourcePath">The resource path used to create the resource identifier.</param>
        /// <param name="allowedResourceProviders">Provides a list of allowed resource providers.</param>
        /// <param name="allowedResourceTypes">Provides a dictionary of <see cref="ResourceTypeDescriptor"/> used to validate resource types.</param>
        /// <param name="allowAction">Optional parameter that indicates whether an action name is allowed as part of the resource identifier.</param>
        /// <param name="resourcePathInstance">The parsed resource path.</param>
        /// <returns>True if the resource path is parsed successfully.</returns>
        public static bool TryParse(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction,
            out ResourcePath? resourcePathInstance)
        {
            try
            {
                resourcePathInstance = new ResourcePath(
                    resourcePath,
                    allowedResourceProviders,
                    allowedResourceTypes,
                    allowAction);
                return true;
            }
            catch
            {
                resourcePathInstance = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to retrieve the identifier of the FoundationaLLM instance from a resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path to analyze.</param>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns>True if a valid intance identifier is retrieved successfully.</returns>
        public static bool TryParseInstanceId(string resourcePath, out string? instanceId)
        {
            instanceId = GetInstanceId(resourcePath);
            return instanceId != null;
        }

        /// <summary>
        /// Tries to retrieve the name of the resource provider from a resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path to analyze.</param>
        /// <param name="resourceProvider">The resource provider name.</param>
        /// <returns>True if a valid resource provider name is retrieved successfully.</returns>
        public static bool TryParseResourceProvider(string resourcePath, out string? resourceProvider)
        {
            resourceProvider = GetResourceProvider(resourcePath);
            return resourceProvider != null;
        }

        /// <summary>
        /// Computes the object id of the resource identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id. Provide this if the resource does not have a fully-qualified resource identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider. Provide this if the resource does not have a fully-qualified resource identifier.</param>
        /// <returns>The object id.</returns>
        public string GetObjectId(
            string? instanceId,
            string? resourceProvider)
        {
            if (_instanceId == null && _resourceProvider == null)
            {
                if (_resourceTypeInstances == null
                    || _resourceTypeInstances.Count == 0)
                    throw new ResourceProviderException("The resource path cannot be converted to a fully qualified resource identifier.",
                        StatusCodes.Status400BadRequest);
                else
                    return $"/instances/{instanceId}/providers/{resourceProvider}/{string.Join("/",
                        _resourceTypeInstances.Select(i => i.ResourceId == null ? $"{i.ResourceType}" : $"{i.ResourceType}/{i.ResourceId}").ToArray())}";
            }
            else
            {
                if (_instanceId == null
                    || _resourceProvider == null
                    || _resourceTypeInstances == null
                    || _resourceTypeInstances.Count == 0)
                    throw new ResourceProviderException("The resource path does not represent a fully qualified resource identifier.",
                        StatusCodes.Status400BadRequest);
                else
                    return $"/instances/{_instanceId}/providers/{_resourceProvider}/{string.Join("/",
                        _resourceTypeInstances.Select(i => $"{i.ResourceType}/{i.ResourceId}").ToArray())}";
            }
        }

        /// <summary>
        /// Determines whether the resource path includes another specified resource path.
        /// </summary>
        /// <param name="other">The <see cref="ResourcePath"/> to check for inclusion.</param>
        /// <returns>True if the specified resource path is included in the resource path.</returns>
        public bool IncludesResourcePath(ResourcePath other)
        {
            if (_isRootPath)
                // The other path is included only if it is root.
                return
                    other.IsRootPath;

            if (IsInstancePath)
            {
                // An instance path includes a root path.
                if (other.IsRootPath)
                    return true;

                // An instance path includes another instance path for the same instance id.
                if (other.IsInstancePath)
                    return _instanceId == other.InstanceId;
            }

            // A full path includes a root path or an instance path.
            if (other.IsRootPath || other.IsInstancePath)
                return true;

            // A full path does not include a full path that is longer than it.
            if (other.ResourceTypeInstances.Count > _resourceTypeInstances.Count)
                return false;

            for (int i = 0; i < other.ResourceTypeInstances.Count; i++)
            {
                if (!_resourceTypeInstances[i].Includes(other.ResourceTypeInstances[i]))
                    return false;
            }

            return true;
        }

        private void ParseResourcePath(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction,
            out bool rootPath,
            out string? instanceId,
            out string? resourceProvider,
            out List<ResourceTypeInstance> resourceTypeInstances)
        {
            try
            {
                instanceId = null;
                resourceProvider = null;
                resourceTypeInstances = [];
                rootPath = false;

                // Check if we deal with a root path ("/").
                if (resourcePath == "/")
                {
                    rootPath = true;
                    return;
                }

                var tokens = GetPathTokens(resourcePath);

                int currentIndex = 0;

                // Retrieve the instance id if present.
                if (tokens[0] == INSTANCE_TOKEN)
                {
                    instanceId = tokens[1];

                    // The instance id must be a valid GUID.
                    if (!Guid.TryParse(instanceId, out _))
                        throw new Exception();
                    currentIndex = 2;
                }

                // An instance identifier is a valid resource path.
                if (currentIndex >= tokens.Length)
                    return;

                // Retrieve the resource provider if present.
                if (tokens[currentIndex] == RESOURCE_PROVIDER_TOKEN)
                {
                    resourceProvider = tokens[currentIndex + 1];

                    // Raise an exception if the resource provider name is not a valid FoundationaLLM resource provider.
                    if (!allowedResourceProviders.Contains(resourceProvider))
                        throw new Exception();

                    currentIndex += 2;
                }
                else if (currentIndex != 0)
                    // If an instance id token is present, the resource provider token must be present as well.
                    throw new Exception();

                // There must be at least one resource type after instance id and/or resource provider.
                if (currentIndex >= tokens.Length)
                    throw new Exception();

                var currentResourceTypes = allowedResourceTypes;

                while (currentIndex < tokens.Length)
                {
                    if (currentResourceTypes == null
                        || !currentResourceTypes.TryGetValue(tokens[currentIndex], out ResourceTypeDescriptor? currentResourceType))
                        throw new Exception();

                    var resourceTypeInstance = new ResourceTypeInstance(tokens[currentIndex]);
                    resourceTypeInstances.Add(resourceTypeInstance);

                    if (currentIndex + 1 == tokens.Length)
                        // No more tokens left, which means we have a resource type instance without actions or subtypes.
                        // This will be used by resource providers to retrieve all resources of a specific resource type.
                        break;

                    // Check if the next token is an action or a resource id.
                    // The only way to determine is by matching the token with the list of supported actions.
                    // If there is not match, the token is considered to be a resource identifier.
                    var action = currentResourceType.Actions.FirstOrDefault(a => a.Name == tokens[currentIndex + 1]);
                    if (action != null)
                    {
                        // The next token is an action

                        // If actions are not allowed or the action is not allowed on a resource type, raise an exception.
                        if (!allowAction
                            || !action.AllowedOnResourceType)
                            throw new Exception();

                        resourceTypeInstance.Action = tokens[currentIndex + 1];

                        // It must be the last token
                        if (currentIndex + 2 == tokens.Length)
                            break;
                        else
                            throw new Exception();
                    }
                    else
                    {
                        // The next token is a resource identifier
                        resourceTypeInstance.ResourceId = tokens[currentIndex + 1];

                        // Check if the token after it exists and is an action.
                        if (currentIndex + 2 <= tokens.Length - 1)
                        {
                            // Check if the next token is an action or a resource id.
                            // The only way to determine is by matching the token with the list of supported actions.
                            // If there is not match, the token is considered to be a resource identifier.
                            var action2 = currentResourceType.Actions.FirstOrDefault(a => a.Name == tokens[currentIndex + 2]);
                            if (action2 != null)
                            {
                                // The token represents an action.

                                // If actions are not allowed or the action is not allowed on a resource, raise an exception.
                                if (!allowAction
                                    || !action2.AllowedOnResource)
                                    throw new Exception();

                                resourceTypeInstance.Action = tokens[currentIndex + 2];

                                // It must be the last token
                                if (currentIndex + 2 == tokens.Length - 1)
                                    break;
                                else
                                    throw new Exception();
                            }
                        }
                    }

                    currentResourceTypes = currentResourceType.SubTypes;
                    currentIndex += 2;
                }
            }
            catch
            {
                throw new ResourceProviderException(
                    $"The resource path [{resourcePath}] is invalid.",
                    StatusCodes.Status400BadRequest);
            }
        }

        private static string[] GetPathTokens(string resourcePath)
        {
            // Resource path cannot be null, empty or whitespace.
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new Exception();

            // Remove leading slash if present.
            if (resourcePath.StartsWith('/'))
                resourcePath = resourcePath[1..];

            var tokens = resourcePath.Split('/', StringSplitOptions.TrimEntries);

            // None of the tokens can be null, empty or whitespace.
            if (tokens.Any(t => string.IsNullOrWhiteSpace(t)))
                throw new Exception();

            return tokens;
        }

        private static string? GetResourceProvider(
            string resourcePath)
        {
            try
            {
                var tokens = GetPathTokens(resourcePath);

                var startIndex = tokens[0] == INSTANCE_TOKEN
                    ? 2
                    : 0;

                if (tokens[startIndex] == RESOURCE_PROVIDER_TOKEN)
                {
                    if (ResourceProviderNames.All.Contains(tokens[startIndex + 1]))
                        return tokens[startIndex + 1];
                }
            }
            catch
            {
            }

            return null;
        }

        private static string? GetInstanceId(
            string resourcePath)
        {
            try
            {
                var tokens = GetPathTokens(resourcePath);

                if (tokens[0] == INSTANCE_TOKEN)
                {
                    if (Guid.TryParse(tokens[1], out _))
                        return tokens[1];
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
