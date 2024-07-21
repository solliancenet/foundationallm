namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Language Model provider constants.
    /// </summary>
    public static class LanguageModelProviders
    {
        /// <summary>
        /// Microsoft
        /// </summary>
        public const string MICROSOFT = "microsoft";

        /// <summary>
        /// OpenAI
        /// </summary>
        public const string OPENAI = "openai";

        /// <summary>
        /// AzureML
        /// </summary>
        public const string AZUREML = "azureml";

        /// <summary>
        /// All providers.
        /// </summary>
        public readonly static string[] All = [MICROSOFT, OPENAI, AZUREML];
    }
}
