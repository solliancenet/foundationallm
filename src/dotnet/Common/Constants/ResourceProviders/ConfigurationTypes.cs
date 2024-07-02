namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// String constants for the types of configurations.
    /// </summary>
    public class ConfigurationTypes
    {
        /// <summary>
        /// Basic configuration without practical functionality. Used as base for all other configurations.
        /// </summary>
        public const string Basic = "basic";

        /// <summary>
        /// Azure App Configuration key value.
        /// </summary>
        public const string AppConfigurationKeyValue = "appconfiguration-key-value";

        /// <summary>
        /// Azure App Configuration key vault reference.
        /// </summary>
        public const string AppConfigurationKeyVaultReference = "appconfiguration-key-vault-reference";

        /// <summary>
        /// Api endpoint resource.
        /// </summary>
        public const string APIEndpoint = "api-endpoint";
    }
}
