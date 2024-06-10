namespace FoundationaLLM.Client.Management
{
    public class ClientSettings
    {
        /// <summary>
        /// Base address for the API
        /// </summary>
        public string APIUrl { get; set; }
        /// <summary>
        /// Scopes the client is requesting
        /// </summary>
        public string APIScope { get; set; }
    }
}
