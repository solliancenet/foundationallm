namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Contains methods to interact with the Management API's endpoints.
    /// </summary>
    public interface IManagementRESTClient
    {
        /// <summary>
        /// Contains methods to interact with the Identity endpoints.
        /// </summary>
        IIdentityRESTClient Identity { get; }

        /// <summary>
        /// Contains methods to interact with the Resources endpoints.
        /// </summary>
        IResourceRESTClient Resources { get; }

        /// <summary>
        /// Contains methods to interact with the Status endpoints.
        /// </summary>
        IStatusRESTClient Status { get; }
    }
}
