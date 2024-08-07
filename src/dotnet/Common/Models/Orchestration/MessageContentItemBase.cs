﻿using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Base message content item model.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(OpenAITextMessageContentItem), MessageContentItemTypes.Text)]
    [JsonDerivedType(typeof(OpenAIImageFileMessageContentItem), MessageContentItemTypes.ImageFile)]
    [JsonDerivedType(typeof(OpenAIFilePathContentItem), MessageContentItemTypes.FilePath)]
    public class MessageContentItemBase
    {
        /// <summary>
        /// The type of the message content item.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-100)]
        public virtual string? Type { get; set; }
    }
}
