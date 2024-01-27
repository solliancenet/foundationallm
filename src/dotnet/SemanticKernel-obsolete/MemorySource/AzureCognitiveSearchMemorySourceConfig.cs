namespace FoundationaLLM.SemanticKernel.MemorySource
{
    /// <summary>
    /// The Azure Cognitive Search memory source configuration class.
    /// </summary>
    public class AzureCognitiveSearchMemorySourceConfig
    {
        /// <summary>
        /// The list of faceted query memory sources.
        /// </summary>
        public required List<FacetedQueryMemorySource> FacetedQueryMemorySources { get; set; }
    }

    /// <summary>
    /// The FacetedQueryMemorySource class.
    /// </summary>
    public class FacetedQueryMemorySource
    {
        /// <summary>
        /// The memory search filter.
        /// </summary>
        public required string Filter { get; init; }

        /// <summary>
        /// The list of faceted query facets.
        /// </summary>
        public required List<FacetedQueryFacet> Facets { get; init; }

        /// <summary>
        /// The total amount of memory templates.
        /// </summary>
        public required string TotalCountMemoryTemplate { get; init; }
    }

    /// <summary>
    /// The FacetedQueryFacet class.
    /// </summary>
    public class FacetedQueryFacet
    {
        /// <summary>
        /// The faceted query facet.
        /// </summary>
        public required string Facet { get; init; }

        /// <summary>
        /// The amount of memory templates.
        /// </summary>
        public required string CountMemoryTemplate { get; init; }
    }
}
