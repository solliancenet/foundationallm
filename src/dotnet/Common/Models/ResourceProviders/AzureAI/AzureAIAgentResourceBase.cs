using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureAI
{
    /// <summary>
    /// Basic model for Azure AI Agent Service resources managed by the FoundationaLLM.AzureAI resource manager.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(AzureAIAgentConversationMapping), AzureAITypes.AgentConversationMapping)]
    [JsonDerivedType(typeof(AzureAIAgentFileMapping), AzureAITypes.AgentFileMapping)]
    public class AzureAIAgentResourceBase : AzureCosmosDBResource
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The logical partition key for the conversation mapping.
        /// </summary>
        /// <remarks>
        /// This property is used by storage providers that support partitioning of data (e.g. Azure Cosmos DB).
        /// </remarks>
        public string PartitionKey =>
            $"{UPN.NormalizeUserPrincipalName()}-{InstanceId}";

        /// <summary>
        /// The Azure AI Foundry project connection string.
        /// </summary>
        public required string ProjectConnectionString { get; set; }
    }
}
