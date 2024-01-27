namespace FoundationaLLM.Common.Models.Cache
{
    /// <summary>
    /// Contains the result of a cache refresh operation.
    /// </summary>
    public class APICacheRefreshResult
    {
        /// <summary>
        /// Details of the cache refresh operation from the called API.
        /// </summary>
        public string? Detail { get; set; }
        /// <summary>
        /// Indicates whether the cache refresh operation was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
