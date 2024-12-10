using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public class AuthorizationResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AuthorizationResourceTypeNames.RoleAssignments,
                new ResourceTypeDescriptor(
                        AuthorizationResourceTypeNames.RoleAssignments,
                        typeof(RoleAssignment))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(RoleAssignment)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.Filter, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(RoleAssignmentQueryParameters)], [typeof(ResourceProviderGetResult<RoleAssignment>)])
                        ])
                    ]
                }
            },
            {
                AuthorizationResourceTypeNames.RoleDefinitions,
                new ResourceTypeDescriptor(
                        AuthorizationResourceTypeNames.RoleDefinitions,
                        typeof(RoleDefinition))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(RoleDefinition)])
                    ],
                    Actions = []
                }
            }
        };
    }
}
