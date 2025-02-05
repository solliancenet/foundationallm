namespace FoundationaLLM.Common.Constants.OpenAI
{
    /// <summary>
    /// Provides the names of the parameters that can be used to create OpenAI agent capabilities.
    /// </summary>
    /// <remarks>
    /// The constants are used by the callers of the <see cref="IGatewayServiceClient"/> implementations.
    /// </remarks>
    public static class OpenAIAgentCapabilityParameterNames
    {
        #region Requests

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant.
        /// </summary>
        public const string CreateOpenAIAssistant = "OpenAI.Assistants.Assistant.Create";

        /// <summary>
        /// Indicates the need to create a vector store for an existing OpenAI assistant.
        /// </summary>
        public const string CreateOpenAIAssistantVectorStore = "OpenAI.Assistants.Assistant.VectorStore.Create";

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant thread.
        /// </summary>
        public const string CreateOpenAIAssistantThread = "OpenAI.Assistants.Thread.Create";

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant file.
        /// </summary>
        public const string CreateOpenAIFile = "OpenAI.File.Create";

        /// <summary>
        /// Indicates the need to add an existing OpenAI assistant file to an OpenAI assistant vector store.
        /// </summary>
        public const string AddOpenAIFileToVectorStore = "OpenAI.File.AddToVectorStore";

        /// <summary>
        /// Indicates the need to remove an existing OpenAI assistant file from an OpenAI assistant vector store.
        /// </summary>
        public const string RemoveOpenAIFileFromVectorStore = "OpenAI.File.RemoveFromVectorStore";

        /// <summary>
        /// Indicates the need to add an existing OpenAI assistant file to an OpenAI assistant code interpreter tool.
        /// </summary>
        public const string AddOpenAIFileToCodeInterpreter = "OpenAI.File.AddToCodeInterpreter";

        /// <summary>
        /// Indicates the need to remove an existing OpenAI assistant file from an OpenAI assistant code interpreter tool.
        /// </summary>
        public const string RemoveOpenAIFileFromCodeInterpreter = "OpenAI.File.RemoveFromCodeInterpreter";
        #endregion

        #region Inputs

        /// <summary>
        /// Provides the prompt used by the OpenAI assistant.
        /// </summary>
        public const string OpenAIAssistantPrompt = "OpenAI.Assistants.Assistant.Prompt";

        /// <summary>
        /// Provides the Azure OpenAI endpoint used to manage Open AI assistants.
        /// </summary>
        public const string OpenAIEndpoint = "OpenAI.Endpoint";

        /// <summary>
        /// Provides the model deployment name used by the OpenAI assistant.
        /// </summary>
        public const string OpenAIModelDeploymentName = "OpenAI.ModelDeploymentName";

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
        /// Provides the identifier of an existing OpenAI assistant.
        /// </summary>
        public const string OpenAIAssistantId = "OpenAI.Assistants.Assistant.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant thread.
        /// </summary>
        public const string OpenAIAssistantThreadId = "OpenAI.Assistants.Thread.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant file.
        /// </summary>
        public const string OpenAIFileId = "OpenAI.Files.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant vector store.
        /// </summary>
        public const string OpenAIVectorStoreId = "OpenAI.VectorStore.Id";

        /// <summary>
        /// Indicates whether the Open AI assistant file vectorization or file removal process completed successfully.
        /// </summary>
        public const string OpenAIFileActionOnVectorStoreSuccess = "OpenAI.File.FileActionOnVectorStoreSuccess";

        /// <summary>
        /// Indicates whether the Open AI assistant file addition or file removal for the code interpreter completed successfully.
        /// </summary>
        public const string OpenAIFileActionOnCodeInterpreterSuccess = "OpenAI.File.FileActionOnCodeInterpreterSuccess";

        #endregion
    }
}
