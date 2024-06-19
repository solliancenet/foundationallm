using System.Net.Http.Headers;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's Attachments endpoints.
    /// </summary>
    public interface IAttachmentRESTClient
    {
        /// <summary>
        /// Uploads a file attachment to the Core API.
        /// </summary>
        /// <param name="fileStream">The file contents of the new Attachment resource.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="contentType">The Content-Type header value of a valid mime type that is used
        /// to create a new <see cref="MediaTypeHeaderValue"/> as part of the
        /// <see cref="MultipartFormDataContent"/> sent to the API endpoint.</param>
        /// <returns>The Object ID of the Attachment Resource Provider created from the filestream.</returns>
        Task<string> UploadAttachmentAsync(Stream fileStream, string fileName, string contentType);
    }
}
