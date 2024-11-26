namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Contains constants for environment variables used by the application.
    /// </summary>
    public static class EnvironmentVariables
    {
        /// <summary>
        /// The client id of the user assigned managed identity.
        /// </summary>
        public const string AzureClientId = "AZURE_CLIENT_ID";

        /// <summary>
        /// The build version of the container. This is also used for the app version used
        /// to validate the minimum version of the app required to use certain configuration entries.
        /// </summary>
        public const string FoundationaLLM_Version = "FOUNDATIONALLM_VERSION";

        /// <summary>
        /// The key for the FoundationaLLM:AppConfig:ConnectionString environment variable.
        /// This allows the caller to connect to the Azure App Configuration service.
        /// </summary>
        public const string FoundationaLLM_AppConfig_ConnectionString = "FoundationaLLM_AppConfig_ConnectionString";

        /// <summary>
        /// They key for the FoundationaLLM:Configuration:KeyVaultURI environment variable.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_KeyVaultURI = "FoundationaLLM_AuthorizationAPI_KeyVaultURI";

        /// <summary>
        /// Indicates whether the application is running in an end-to-end test environment. The string value should be either "true" or "false".
        /// </summary>
        public const string FoundationaLLM_Environment = "FOUNDATIONALLM_ENVIRONMENT";

        /// <summary>
        /// The name of the Azure Container Apps (AKS) replica.
        /// </summary>
        /// <remarks>
        /// This environment variable is available only in the Azure Container Apps environment.
        /// </remarks>
        public const string ACA_Container_App_Replica_Name = "CONTAINER_APP_REPLICA_NAME";

        /// <summary>
        /// The name of the Azure Kubernetes Service (AKS) pod.
        /// </summary>
        /// <remarks>
        /// This environment variable is available only in the Azure Kubernetes Service environment.
        /// </remarks>
        public const string AKS_Pod_Name = "POD_NAME";
    }
}
