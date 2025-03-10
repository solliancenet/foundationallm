﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides details about a file mapping between FoundationaLLM and Azure OpenAI.
    /// </summary>
    public class AzureOpenAIFileMapping : AzureOpenAIResourceBase
    {
        /// <summary>
        /// The FoundationaLLM.Attachment resource object id.
        /// </summary>
        [JsonPropertyName("file_object_id")]
        public required string FileObjectId { get; set; }

        /// <summary>
        /// The original file name of the file.
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// The content type of the file.
        /// </summary>
        [JsonPropertyName("file_content_type")]
        public required string FileContentType { get; set; }

        /// <summary>
        /// Indicates whether the file requires vectorization or not.
        /// </summary>
        [JsonPropertyName("file_requires_vectorization")]
        public bool FileRequiresVectorization { get; set; } = false;

        /// <summary>
        /// The OpenAI file id associated with the FoundationaLLM file (attachment) id.
        /// </summary>
        [JsonPropertyName("openai_file_id")]
        public required string OpenAIFileId { get; set; }

        /// <summary>
        /// The time when the file was uploaded to OpenAI.
        /// </summary>
        [JsonPropertyName("openai_file_uploaded_on")]
        public DateTimeOffset? OpenAIFileUploadedOn { get; set; }

        /// <summary>
        /// The time when the file was generated by OpenAI.
        /// </summary>
        [JsonPropertyName("openai_assistants_file_generated_on")]
        public DateTimeOffset? OpenAIAssistantsFileGeneratedOn { get; set; }

        /// <summary>
        /// The OpenAI vector store id holding the vectorized content of the OpenAI file.
        /// </summary>
        [JsonPropertyName("openai_vector_store_id")]
        public string? OpenAIVectorStoreId { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public AzureOpenAIFileMapping() =>
            Type = AzureOpenAITypes.FileMapping;
    }
}
