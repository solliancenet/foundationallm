using FoundationaLLM.Common.Clients;

namespace FoundationaLLM.Common.Constants.AzureAI
{
    /// <summary>
    /// Provides the names of the parameters that can be used to create Azure AI Agent Service capabilities.
    /// </summary>
    /// <remarks>
    /// The constants are used by the callers of the <see cref="GatewayServiceClient"/> implementations.
    /// </remarks>
    public class AzureAIAgentServiceCapabilityParameterNames
    {
        #region Requests

        /// <summary>
        /// Indicates the need to create a new Azure AI agent.
        /// </summary>
        public const string CreateAgent = "AzureAI.AgentService.Agent.Create";

        /// <summary>
        /// Indicates the need to create a vector store in the Azure AI Agent Service.
        /// </summary>
        public const string CreateVectorStore = "AzureAI.AgentService.VectorStore.Create";

        /// <summary>
        /// Indicates the need to create a new Azure AI agent thread.
        /// </summary>
        public const string CreateThread = "AzureAI.AgentService.Thread.Create";

        /// <summary>
        /// Indicates the need to create a new Azure AI agent file.
        /// </summary>
        public const string CreateFile = "AzureAI.AgentService.File.Create";

        /// <summary>
        /// Indicates the need to add an existing Azure AI file to an existing Azure AI vector store.
        /// </summary>
        public const string AddFileToVectorStore = "AzureAI.AgentService.File.AddToVectorStore";

        /// <summary>
        /// Indicates the need to remove an existing Azure AI agent file from an Azure AI vector store.
        /// </summary>
        public const string RemoveFileFromVectorStore = "AzureAI.AgentService.File.RemoveFromVectorStore";

        /// <summary>
        /// Indicates the need to add an existing Azure AI agent file to an Azure AI agent code interpreter tool.
        /// </summary>
        public const string AddFileToCodeInterpreter = "AzureAI.AgentService.Agent.File.AddToCodeInterpreter";

        /// <summary>
        /// Indicates the need to remove an existing Azure AI agent file from an Azure AI agent code interpreter tool.
        /// </summary>
        public const string RemoveFileFromCodeInterpreter = "AzureAI.AgentService.Agent.File.RemoveFromCodeInterpreter";
        #endregion

        #region Inputs

        /// <summary>
        /// Provides the prompt used by the Azure AI agent.
        /// </summary>
        public const string AgentPrompt = "AzureAI.AgentService.Agent.Prompt";

        /// <summary>
        /// Provides the Azure AI project connection string.
        /// </summary>
        public const string ProjectConnectionString = "AzureAI.Project.ConnectionString";
              
        /// <summary>
        /// Provides the model deployment name used by the Azure AI agent.
        /// </summary>
        public const string AzureAIModelDeploymentName = "AzureAI.ModelDeploymentName";

        /// <summary>
        /// The object identifier of the FoundationaLLM attachment resource.
        /// </summary>
        public const string AttachmentObjectId = "FoundationaLLM.Attachment.ObjectId";

        /// <summary>
        /// The object identifier of the FoundationaLLM agent file resource.
        /// </summary>
        public const string AgentFileObjectId = "FoundationaLLM.Agent.File.ObjectId";

        #endregion

        #region Outputs

        /// <summary>
        /// Provides the identifier of an existing Azure AI agent.
        /// </summary>
        public const string AgentId = "AzureAI.AgentService.Agent.Id";

        /// <summary>
        /// Provides the identifier of an existing AzureAI agent thread.
        /// </summary>
        public const string ThreadId = "AzureAI.AgentService.Thread.Id";

        /// <summary>
        /// Provides the identifier of an existing Azure AI file.
        /// </summary>
        public const string FileId = "AzureAI.AgentService.File.Id";

        /// <summary>
        /// Provides the identifier of an existing Azure AI agent vector store.
        /// </summary>
        public const string VectorStoreId = "AzureAI.AgentService.VectorStore.Id";

        /// <summary>
        /// Indicates whether the Azure AI agent file vectorization or file removal process completed successfully.
        /// </summary>
        public const string FileActionOnVectorStoreSuccess = "AzureAI.AgentService.File.FileActionOnVectorStoreSuccess";

        /// <summary>
        /// Indicates whether the Open AI agent file addition or file removal for the code interpreter completed successfully.
        /// </summary>
        public const string FileActionOnCodeInterpreterSuccess = "AzureAI.AgentService.File.FileActionOnCodeInterpreterSuccess";

        #endregion
    }
}
