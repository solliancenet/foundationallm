namespace FoundationaLLM.Gateway.Models.Configuration
{
    /// <summary>
    /// Provides settings for the Gateway core service.
    /// </summary>
    public class GatewayCoreSettings
    {
        /// <summary>
        /// The semicolon separated list of Azure Open AI endpoints used by the Gateway core service.
        /// </summary>
        public required string AzureOpenAIAccounts { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum time in seconds allowed for an Azure OpenAI Assistants vectorization process to complete.
        /// </summary>
        public required int AzureOpenAIAssistantsMaxVectorizationTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum time in seconds allowed for an Azure AI Agent Service vectorization process to complete.
        /// </summary>
        public required int AzureAIAgentServiceMaxVectorizationTimeSeconds { get; set; }

    }
}
