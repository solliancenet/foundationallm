using FoundationaLLM.Common.Models.Orchestration.Response;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Represents an item in the message history.
    /// </summary>
    /// <param name="sender">The sender of the message (e.g., "Agent", "User")</param>
    /// <param name="text">The message text.</param>
    /// <param name="textRewrite">The rewritten message text.</param>
    public class MessageHistoryItem(string sender, string text, string? textRewrite)
    {
        /// <summary>
        /// The sender of the message (e.g. "Agent", "User").
        /// </summary>
        [JsonPropertyName("sender")]
        public string Sender { get; set; } = sender;
        /// <summary>
        /// The message text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; } = text;

        /// <summary>
        /// Gets or sets the rewritten text.
        /// </summary>
        /// <remarks>
        /// The rewritten text is filled in if the agent's text rewriting feature is enabled.
        /// If set, it contains an unambiguous version of the original text.
        /// </remarks>
        [JsonPropertyName("text_rewrite")]
        public string? TextRewrite { get; set; } = textRewrite;

        /// <summary>
        /// Gets or sets the list <see cref="ContentArtifact"/> objects.
        /// </summary>
        /// <remarks>
        /// Not all content artifacts are loaded.
        /// The content of the list depends on the content artifact types configured in the message history settings of the agent.
        /// </remarks>
        [JsonPropertyName("content_artifacts")]
        public List<ContentArtifact>? ContentArtifacts { get; set; }

        /// <summary>
        /// Gets or sets the list of attachments for the message.
        /// </summary>
        [JsonPropertyName("attachments")]
        public List<AttachmentDetail> Attachments { get; set; } = [];
    }
}
