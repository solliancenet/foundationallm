namespace FoundationaLLM.AuthorizationEngine.Models.Configuration
{
    /// <summary>
    /// Configuration settings for the authorization core.
    /// </summary>
    public class AuthorizationCoreSettings
    {
        /// <summary>
        /// The list of instance identifiers of the FoundationaLLM instances that are managed by the authorization core.
        /// </summary>
        public List<string> InstanceIds { get; set; } = [];
    }
}
