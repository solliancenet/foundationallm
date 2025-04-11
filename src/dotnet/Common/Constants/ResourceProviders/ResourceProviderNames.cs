using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants of the resource provider names.
    /// </summary>
    public static class ResourceProviderNames
    {
        /// <summary>
        /// The name of the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        public const string FoundationaLLM_Vectorization = "FoundationaLLM.Vectorization";

        /// <summary>
        /// The name of the FoundationaLLM.Agent resource provider.
        /// </summary>
        public const string FoundationaLLM_Agent = "FoundationaLLM.Agent";

        /// <summary>
        /// The name of the FoundationaLLM.Configuration resource provider.
        /// </summary>
        public const string FoundationaLLM_Configuration = "FoundationaLLM.Configuration";

        /// <summary>
        /// The name of the FoundationaLLM.Prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_Prompt = "FoundationaLLM.Prompt";

        /// <summary>
        /// The name of the FoundationaLLM.DataSource resource provider.
        /// </summary>
        public const string FoundationaLLM_DataSource = "FoundationaLLM.DataSource";

        /// <summary>
        /// The name of the FoundationaLLM.Attachment resource provider.
        /// </summary>
        public const string FoundationaLLM_Attachment = "FoundationaLLM.Attachment";

        /// <summary>
        /// The name of the FoundationaLLM.Authorization resource provider.
        /// </summary>
        public const string FoundationaLLM_Authorization = "FoundationaLLM.Authorization";

        /// <summary>
        /// The name of the FoundationaLLM.AIModel resource provider.
        /// </summary>
        public const string FoundationaLLM_AIModel = "FoundationaLLM.AIModel";

        /// <summary>
        /// The name of the FoundationaLLM.AzureOpenAI resource provider.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI = "FoundationaLLM.AzureOpenAI";

        /// <summary>
        /// The name of the FoundationaLLM.AzureAI resource provider.
        /// </summary>
        public const string FoundationaLLM_AzureAI = "FoundationaLLM.AzureAI";

        /// <summary>
        /// The name of the FoundationaLLM.Conversation resource provider.
        /// </summary>
        public const string FoundationaLLM_Conversation = "FoundationaLLM.Conversation";

        /// <summary>
        /// The name of the FoundationaLLM.DataPipeline resource provider.
        /// </summary>
        public const string FoundationaLLM_DataPipeline = "FoundationaLLM.DataPipeline";

        /// <summary>
        /// The name of the FoundationaLLM.Plugin resource provider.
        /// </summary>
        public const string FoundationaLLM_Plugin = "FoundationaLLM.Plugin";

        /// <summary>
        /// Contains all the resource provider names.
        /// </summary>
        public readonly static ImmutableList<string> All = [
            FoundationaLLM_Vectorization,
            FoundationaLLM_Agent,
            FoundationaLLM_Configuration,
            FoundationaLLM_Prompt,
            FoundationaLLM_DataSource,
            FoundationaLLM_Attachment,
            FoundationaLLM_Authorization,
            FoundationaLLM_AIModel,
            FoundationaLLM_AzureOpenAI,
            FoundationaLLM_Conversation,
            FoundationaLLM_DataPipeline,
            FoundationaLLM_Plugin
        ];
    }
}
