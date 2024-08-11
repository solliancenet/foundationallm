﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about a file used by OpenAI assistants.
    /// </summary>
    public class FileMapping
    {
        /// <summary>
        /// The FoundationaLLM.Attachment resource object id.
        /// </summary>
        [JsonPropertyName("foundationallm_attachment_object_id")]
        public required string FoundationaLLMAttachmentObjectId { get; set; }

        /// <summary>
        /// The original file name of the file.
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// The content type of the file.
        /// </summary>
        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }

        /// <summary>
        /// The OpenAI file id associated with the FoundationaLLM file (attachment) id.
        /// </summary>
        [JsonPropertyName("openai_file_id")]
        public string? OpenAIFileId { get; set; }

        /// <summary>
        /// The time when the file was uploaded to OpenAI.
        /// </summary>
        [JsonPropertyName("openai_file_uploaded_on")]
        public DateTimeOffset? OpenAIFileUploadedOn { get; set; }
    }
}