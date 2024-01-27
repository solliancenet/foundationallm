namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Calls endpoints available on all hub API services.
    /// </summary>
    public interface IHubAPIService
    {
        /// <summary>
        /// Gets the status of the Hub API service.
        /// </summary>
        /// <returns></returns>
        Task<string> Status();
    }
}
