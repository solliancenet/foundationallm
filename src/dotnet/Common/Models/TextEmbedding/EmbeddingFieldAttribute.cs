namespace FoundationaLLM.Common.Models.TextEmbedding;

/// <summary>
/// The embedding field attribute object.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class EmbeddingFieldAttribute : Attribute
{
    /// <summary>
    /// The label associated with the embedding field.
    /// </summary>
    public string? Label { get; set; }
}
