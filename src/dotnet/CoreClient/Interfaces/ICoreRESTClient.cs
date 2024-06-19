namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Contains methods to interact with the Core API's endpoints.
    /// </summary>
    public interface ICoreRESTClient
    {
        /// <summary>
        /// Contains methods to interact with the Sessions endpoints.
        /// </summary>
        ISessionRESTClient Sessions { get; }
        /// <summary>
        /// Contains methods to interact with the Attachments endpoints.
        /// </summary>
        IAttachmentRESTClient Attachments { get; }
        /// <summary>
        /// Contains methods to interact with the Branding endpoints.
        /// </summary>
        IBrandingRESTClient Branding { get; }
        /// <summary>
        /// Contains methods to interact with the Orchestration endpoints.
        /// </summary>
        ICompletionRESTClient Completions { get; }
        /// <summary>
        /// Contains methods to interact with the Status endpoints.
        /// </summary>
        IStatusRESTClient Status { get; }
        /// <summary>
        /// Contains methods to interact with the UserProfiles endpoints.
        /// </summary>
        IUserProfileRESTClient UserProfiles { get; }
    }
}
