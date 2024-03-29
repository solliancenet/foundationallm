﻿using System.Text.Json.Serialization;
using UglyToad.PdfPig.Outline;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization artifact.
    /// </summary>
    public class VectorizationArtifact
    {
        /// <summary>
        /// The type of the vectorization artifact.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationArtifactType Type { get; set; }

        /// <summary>
        /// The canonical identifier of the vectorization artifact.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName ("canonical_id")]
        public string? CanonicalId { get; set; }

        /// <summary>
        /// The position of the vectorization artifact.
        /// This is relevant only for artifact types that can have multiple parts.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("position")]
        public int Position { get; set; }

        /// <summary>
        /// The content of the artifact.
        /// </summary>
        [JsonIgnore]
        public string? Content { get; set; }

        /// <summary>
        /// Indicates the need to persist the content of the artifact.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty { get; set; }
    }
}
