using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Agent resource provider.
    /// </summary>
    public static class AgentResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            {
                AgentResourceTypeNames.Agents,
                new ResourceTypeDescriptor(
                        AgentResourceTypeNames.Agents,
                        typeof(AgentBase))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AgentBase>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AgentBase)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], []),
                    ],
                    Actions = [
                        new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                        ]),
                        new ResourceTypeAction(ResourceProviderActions.SetDefault, true, false, [
                            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [], [typeof(ResourceProviderActionResult)])
                        ])
                    ],
                    SubTypes = new()
                    {
                        {
                            AgentResourceTypeNames.AgentAccessTokens,
                            new ResourceTypeDescriptor(
                                    AgentResourceTypeNames.AgentAccessTokens,
                                    typeof(AgentAccessToken))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<AgentAccessToken>)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, [], [typeof(AgentAccessToken)], [typeof(ResourceProviderUpsertResult)]),
                                    new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], [])
                                ],
                                Actions = [
                                    new ResourceTypeAction(ResourceProviderActions.Validate, false, true, [
                                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Read, [], [typeof(AgentAccessTokenValidationRequest)], [typeof(AgentAccessTokenValidationResult)])
                                    ])
                                ]
                            }
                        }
                    }
                }
            },
            {
                AgentResourceTypeNames.Workflows,
                new ResourceTypeDescriptor(
                        AgentResourceTypeNames.Workflows,
                        typeof(Workflow))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<Workflow>)])
                    ],
                    Actions = []
                }
            },
            {
                AgentResourceTypeNames.Tools,
                new ResourceTypeDescriptor(
                        AgentResourceTypeNames.Tools,
                        typeof(Tool))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<Tool>)])
                    ],
                    Actions = []
                }
            }
        };
    }
}
