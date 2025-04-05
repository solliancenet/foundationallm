using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Models;

namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the interface for code session provider services.
    /// </summary>
    public interface ICodeSessionProviderService
    {
        /// <summary>
        /// Gets the name of the code session provider.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates a new code session.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the FoundationaLLM instance.</param>
        /// <param name="agentName">The name of the agent for which the code execution session is created.</param>
        /// <param name="conversationId">The unique identifier of the conversation.</param>
        /// <param name="context">The context in which the code execution session is created. This is usually the name of the agent tool, but it is not limited to that.</param>
        /// <param name="language">The code session programing language.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the user identity information.</param>
        /// <returns>A <see cref="CreateCodeSessionResponse"/> object with the properties of the code execution session.</returns>
        Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string language,
            UnifiedUserIdentity userIdentity);

        /// <summary>
        /// Uploads a file to a code execution session.
        /// </summary>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="fileContent">The stream containing the binary content of the file to upload.</param>
        /// <returns></returns>
        Task<bool> UploadFileToCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            Stream fileContent);

        /// <summary>
        /// Lists files from a code session.
        /// </summary>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <returns>The list of file paths from the code session.</returns>
        Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint);

        /// <summary>
        /// Downloads a file from a code session.
        /// </summary>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <param name="fileName">The name of the file to download.</param>
        /// <param name="filePath">The path to the file to download.</param>
        /// <returns>A stream with the binary content of the file.</returns>
        Task<Stream?> DownloadFileFromCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            string filePath);

        /// <summary>
        /// Deletes all files from a code session.
        /// </summary>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <returns></returns>
        Task DeleteCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint);
    }
}
