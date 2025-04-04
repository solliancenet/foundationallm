﻿using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response
{
    /// <summary>
    /// Base message content item model.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextMessageContentItem), MessageContentItemTypes.Text)]
    [JsonDerivedType(typeof(ImageFileMessageContentItem), MessageContentItemTypes.ImageFile)]
    [JsonDerivedType(typeof(FilePathContentItem), MessageContentItemTypes.FilePath)]
    public class MessageContentItemBase
    {
        /// <summary>
        /// The type of the message content item.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-100)]
        public virtual string? Type { get; set; }

        /// <summary>
        /// The FoundationaLLM agent capability category that generated the message content item.
        /// </summary>
        /// <remarks>
        /// Must be one of the values defined in <see cref="AgentCapabilityCategoryNames"/>.
        /// </remarks>
        [JsonPropertyName("agent_capability_category")]
        [JsonPropertyOrder(-99)]
        public required string AgentCapabilityCategory { get; set; }
    }
}
