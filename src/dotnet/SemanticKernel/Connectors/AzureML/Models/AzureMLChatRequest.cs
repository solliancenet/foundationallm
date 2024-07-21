using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models
{
    /// <summary>
    /// The AzureML chat request object definition.
    /// </summary>
    public class AzureMLChatRequest
    {
        /// <summary>
        /// Input data for the AzureML model request.
        /// </summary>
        [JsonPropertyName("input_data")]
        public required AzureMLInputData InputData { get; set; }


        public static AzureMLChatRequest FromChatHistoryAndExecutionSettings(
            ChatHistory chatHistory, 
            AzureMLPromptExecutionSettings executionSettings)
        {            
            var request = new AzureMLChatRequest() { InputData = new AzureMLInputData()};
            foreach (var message in chatHistory)
            {
                request.AddChatMessage(message);
            }
            AddConfiguration(executionSettings, request);
            return request;
        }       

        /// <summary>
        /// Adds a chat message to the input data.
        /// </summary>
        /// <param name="message"></param>
        public void AddChatMessage(ChatMessageContent message)
        {
            if (message is not null)
                this.InputData.InputString.Add(new AzureMLChatMessage() { Role = message.Role.ToString().ToLower(), Content = message.ToString() });
        }

        /// <summary>
        /// Sets parameters based on the execution settings.
        /// </summary>
        /// <param name="executionSettings">The execution settings.</param>
        /// <param name="request">The request to update the parameters.</param>
        private static void AddConfiguration(AzureMLPromptExecutionSettings executionSettings, AzureMLChatRequest request)
        {
            if (executionSettings is not null)
            {
                if (executionSettings.Temperature is not null)
                    request.InputData.Parameters["temperature"] = executionSettings.Temperature;
                if (executionSettings.MaxNewTokens is not null)
                    request.InputData.Parameters["max_new_tokens"] = executionSettings.MaxNewTokens;
                if (executionSettings.TopP is not null)
                    request.InputData.Parameters["top_p"] = executionSettings.TopP;
                if (executionSettings.TopK is not null)
                    request.InputData.Parameters["top_k"] = executionSettings.TopK;
            }
        }
    }
}
