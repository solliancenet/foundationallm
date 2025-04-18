﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureAI
{
    /// <summary>
    /// Provides details about a file mapping between FoundationaLLM and the Azure AI Agent Service.
    /// </summary>
    public class AzureAIAgentFileMapping : AzureAIAgentResourceBase, IFileMapping
    {
        /// <inheritdoc/>
        [JsonPropertyName("file_object_id")]
        public required string FileObjectId { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("file_content_type")]
        public required string FileContentType { get; set; }

        /// <inheritdoc/>
        [JsonPropertyName("file_requires_vectorization")]
        public bool FileRequiresVectorization { get; set; } = false;

        /// <summary>
        /// The Azure AI Agent Service file id associated with the FoundationaLLM file (attachment) id.
        /// </summary>
        [JsonPropertyName("azureai_agent_file_id")]
        public required string AzureAIAgentFileId { get; set; }

        /// <summary>
        /// The time when the file was uploaded to the Azure AI Agent service.
        /// </summary>
        [JsonPropertyName("azureai_agent_file_uploaded_on")]
        public DateTimeOffset? AzureAIAgentFileUploadedOn { get; set; }

        /// <summary>
        /// The time when the file was generated by the Azure AI Agent service.
        /// </summary>
        [JsonPropertyName("azureai_agent_file_generated_on")]
        public DateTimeOffset? AzureAIAgentFileGeneratedOn { get; set; }

        /// <summary>
        /// The OpenAI vector store id holding the vectorized content of the OpenAI file.
        /// </summary>
        [JsonPropertyName("azureai_agent_vector_store_id")]
        public string? AzureAIAgentVectorStoreId { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public AzureAIAgentFileMapping() =>
            Type = AzureAITypes.AgentFileMapping;
    }
}
