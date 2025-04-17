using FoundationaLLM.Common.Models.ResourceProviders;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata that applies to all resource providers.
    /// </summary>
    public class ResourceProviderMetadata
    {
        /// <summary>
        /// Provides the list of all the resource provider metadata definitions.
        /// </summary>
        public readonly static ImmutableList<Dictionary<string, ResourceTypeDescriptor>> AllowedResourceTypes = [
            AgentResourceProviderMetadata.AllowedResourceTypes,
            AIModelResourceProviderMetadata.AllowedResourceTypes,
            AttachmentResourceProviderMetadata.AllowedResourceTypes,
            AuthorizationResourceProviderMetadata.AllowedResourceTypes,
            AzureAIResourceProviderMetadata.AllowedResourceTypes,
            AzureOpenAIResourceProviderMetadata.AllowedResourceTypes,
            ConfigurationResourceProviderMetadata.AllowedResourceTypes,
            ConversationResourceProviderMetadata.AllowedResourceTypes,
            DataPipelineResourceProviderMetadata.AllowedResourceTypes,
            DataSourceResourceProviderMetadata.AllowedResourceTypes,
            PluginResourceProviderMetadata.AllowedResourceTypes,
            PromptResourceProviderMetadata.AllowedResourceTypes
        ];

        /// <summary>
        /// Provides a combined dictionary with all the resource type descriptiors of all resource types.
        /// </summary>
        public readonly static Dictionary<string, ResourceTypeDescriptor> AllAllowedResourceTypes =
            ResourceProviderMetadata.AllowedResourceTypes.SelectMany(d => d).ToDictionary();
    }
}
