// -------------------------------------------------------------------------------
//
// WARNING!
// This file is auto-generated based on the AuthorizationAppConfiguration.json file.
// Do not make changes to this file, as they will be automatically overwritten.
//
// -------------------------------------------------------------------------------
namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Defines all Azure Key vault secret names referred by the Azure App Configuration keys.
    /// </summary>
    public static partial class AuthorizationKeyVaultSecretNames
    {
        /// <summary>
        /// The name of the Azure Key Vault secret providing the name of the Azure Blob Storage account used by the FoundationaLLM.Authorization resource provider.
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Authorization_Storage_AccountName =
            "foundationallm-authorizationapi-storage-accountname";

        /// <summary>
        /// The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Authorization API.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_AppInsightsConnectionString =
            "foundationallm-authorizationapi-appinsights-connectionstring";

        /// <summary>
        /// The name of the Azure Key Vault secret holding the comma-separated list of FoundationaLLM instance ids that are serviced by the Authorization API.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_InstanceIds =
            "foundationallm-authorizationapi-instanceids";

        /// <summary>
        /// The name of the Azure Key Vault secret holding the URL of the Entra ID instance used for authentication.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_Instance =
            "foundationallm-authorizationapi-entra-instance";

        /// <summary>
        /// The name of the Azure Key Vault secret holding the unique identifier of the Entra ID tenant used for authentication.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_TenantId =
            "foundationallm-authorizationapi-entra-tenantid";

        /// <summary>
        /// The name of the Azure Key Vault secret holding the unique identifier of the Entra ID app registration used by the Authorization API to authenticate.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_ClientId =
            "foundationallm-authorizationapi-entra-clientid";
    }
}
