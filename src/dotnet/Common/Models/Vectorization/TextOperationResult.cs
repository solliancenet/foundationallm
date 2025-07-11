﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// The result of a text operation request.
    /// </summary>
    public class TextOperationResult
    {
        /// <summary>
        /// Indicates whether the text operation is still in progress.
        /// When true, the <see cref="OperationId"/> property contains an operation identifier.
        /// </summary>
        [JsonPropertyName("in_progress")]
        public bool InProgress { get; set; }

        /// <summary>
        /// Indicates whether the text operation failed due to an error.
        /// When true, the <see cref="ErrorMessage"/> property contains a message describing the error.
        /// </summary>
        [JsonPropertyName("cancelled")]
        public bool Failed { get; set; }

        /// <summary>
        /// The message describing the error that lead to the cancellation of the operation.
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Optional operation identifier that can be used to retrieve the final result.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// The list of <see cref="TextChunk"/> objects containing the results.
        /// </summary>
        [JsonPropertyName("text_chunks")]
        public IList<TextChunk> TextChunks { get; set; } = [];

        /// <summary>
        /// The number of tokens used during the text operation.
        /// </summary>
        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }

        /// <summary>
        /// Gets or sets the number of text chunks that were successfully processed.
        /// </summary>
        [JsonPropertyName("processed_text_chunks_count")]
        public int ProcessedTextChunksCount { get; set; }
    }
}
