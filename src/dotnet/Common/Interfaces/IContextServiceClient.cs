using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides methods to call the FoundationaLLM Context API service.
    /// </summary>
    public interface IContextServiceClient
    {
        /// <summary>
        /// Calls the Context API service to create a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <param name="fileContentType">The content type of the file to be created.</param>
        /// <param name="fileContent">The binary content of the file.</param>
        /// <returns>A <see cref="ContextServiceResponse{T}"/> instance where <c>T</c> is of type <see cref="ContextFileRecord"/>.</returns>
        Task<ContextServiceResponse<ContextFileRecord>> CreateFile(
            string instanceId,
            string conversationId,
            string fileName,
            string fileContentType,
            Stream fileContent);

        /// <summary>
        /// Calls the Context API service to get the content of a file.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <returns>A <see cref="ContextServiceResponse{T}"/> instance where <c>T</c> is of type <see cref="ContextFileContent"/>.</returns>
        Task<ContextServiceResponse<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId);

        /// <summary>
        /// Calls the Context API service to get a file record.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="fileId">The unique identifier of the file.</param>
        /// <returns>A <see cref="ContextServiceResponse{T}"/> instance where <c>T</c> is of type <see cref="ContextFileRecord"/>.</returns>
        Task<ContextServiceResponse<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId);

        /// <summary>
        /// Calls the Context API service to create a code session.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="context">The context within the conversation in which the code session must be created (e.g., an agent tool name).</param>
        /// <param name="endpointProvider">The name of the code session endpoint provider.</param>
        /// <param name="language">The programming language of the code session.</param>
        /// <returns>A <see cref="ContextServiceResponse{T}"/> instance where <c>T</c> is of type <see cref="CreateCodeSessionResponse"/>.</returns>
        Task<ContextServiceResponse<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string endpointProvider,
            string language);
    }
}
