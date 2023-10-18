namespace FoundationaLLM.Gatekeeper.Core.Models.ContentSafety
{
    /// <summary>
    /// Content filter text analysis restult.
    /// </summary>
    public class AnalyzeTextFilterResult
    {
        /// <summary>
        /// A flag representing if the content is safe or not.
        /// </summary>
        public bool Safe { get; set; }

        /// <summary>
        /// The reason why the content was considered to be unsafe.
        /// </summary>
        public required string Reason { get; set; }
    }
}
