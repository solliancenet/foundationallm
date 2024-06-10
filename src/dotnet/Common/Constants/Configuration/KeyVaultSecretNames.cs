using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Contains constants for the names of all Azure Key Vault secrets.
    /// </summary>
    public static class KeyVaultSecretNames
    {
        /// <summary>
        /// The foundationallm-apis-orchestrationapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_OrchestrationAPI_APIKey = "foundationallm-apis-orchestrationapi-apikey";
        /// <summary>
        /// The foundationallm-app-insights-connection-string Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_App_Insights_Connection_String = "foundationallm-app-insights-connection-string";
        /// <summary>
        /// The foundationallm-apis-agenthubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI_APIKey = "foundationallm-apis-agenthubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-datasourcehubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_DataSourceHubAPI_APIKey = "foundationallm-apis-datasourcehubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-gatekeeperapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_APIKey = "foundationallm-apis-gatekeeperapi-apikey";

        /// <summary>
        /// The foundationallm-apis-gatekeeper-lakeraguard-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_LakeraGuard_APIKey = "foundationallm-apis-gatekeeper-lakeraguard-apikey";
        /// <summary>
        /// The foundationallm-apis-gatekeeper-enkryptguardrails-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_EnkryptGuardrails_APIKey = "foundationallm-apis-gatekeeper-enkryptguardrails-apikey";
        /// <summary>
        /// The foundationallm-apis-gatekeeper-azurecontentsafety-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_APIKey = "foundationallm-apis-gatekeeper-azurecontentsafety-apikey";

        /// <summary>
        /// The foundationallm-apis-gatekeeperintegrationapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey = "foundationallm-apis-gatekeeperintegrationapi-apikey";
        /// <summary>
        /// The foundationallm-apis-langchainapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI_APIKey = "foundationallm-apis-langchainapi-apikey";
        /// <summary>
        /// The foundationallm-apis-prompthubapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI_APIKey = "foundationallm-apis-prompthubapi-apikey";
        /// <summary>
        /// The foundationallm-apis-semantickernelapi-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI_APIKey = "foundationallm-apis-semantickernelapi-apikey";
        /// <summary>
        /// The foundationallm-azurecontentsafety-apikey Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_APIKey = "foundationallm-azurecontentsafety-apikey";
        /// <summary>
        /// The foundationallm-azureopenai-api-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_Api_Key = "foundationallm-azureopenai-api-key";
        /// <summary>
        /// The foundationallm-chat-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_ClientSecret = "foundationallm-chat-entra-clientsecret";
        /// <summary>
        /// The foundationallm-coreapi-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Coreapi_Entra_ClientSecret = "foundationallm-coreapi-entra-clientsecret";
        /// <summary>
        /// The foundationallm-langchain-csvfile-url Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_LangChain_CsvFile_Url = "foundationallm-langchain-csvfile-url";
        /// <summary>
        /// The foundationallm-langchain-sqldatabase-testdb-password Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_LangChain_SQLDatabase_Testdb_Password = "foundationallm-langchain-sqldatabase-testdb-password";
        /// <summary>
        /// The foundationallm-management-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_ClientSecret = "foundationallm-management-entra-clientsecret";
        /// <summary>
        /// The foundationallm-managementapi-entra-clientsecret Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_Managementapi_Entra_ClientSecret = "foundationallm-managementapi-entra-clientsecret";
        /// <summary>
        /// The foundationallm-openai-api-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_OpenAI_Api_Key = "foundationallm-openai-api-key";
        /// <summary>
        /// The foundationallm-semantickernelapi-openai-key Key Vault secret name.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_Key = "foundationallm-semantickernelapi-openai-key";
        /// <summary>
        /// The foundationallm-apis-vectorizationapi-apikey Key Vault secret name.
        /// The API key of the vectorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_APIKey = "foundationallm-apis-vectorizationapi-apikey";
        /// <summary>
        /// The foundationallm-apis-vectorizationworker-apikey Key Vault secret name.
        /// The API key of the vectorization worker API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_APIKey = "foundationallm-apis-vectorizationworker-apikey";
        /// <summary>
        /// The foundationallm-events-azureeventgrid-apikey Key Vault secret name.
        /// The API key for the Azure Event Grid service.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGrid_APIKey = "foundationallm-events-azureeventgrid-apikey";
        /// <summary>
        /// The foundationallm-apis-gatewayapi-apikey Key Vault secret name.
        /// The API key of the Gateway API.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI_APIKey = "foundationallm-apis-gatewayapi-apikey";

        #region Authorization API

        /// <summary>
        /// The foundationallm-authorizationapi-appinsights-connectionstring Key Vault secret name.
        /// The connection string used by OpenTelemetry to connect to App Insights.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_AppInsights_ConnectionString = "foundationallm-authorizationapi-appinsights-connectionstring";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The Entra ID instance.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Entra_Instance = "foundationallm-authorizationapi-entra-instance";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The Entra ID tenant id.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Entra_TenantId = "foundationallm-authorizationapi-entra-tenantid";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// Entra ID client id.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Entra_ClientId = "foundationallm-authorizationapi-entra-clientid";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The Entr ID client secret.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Entra_ClientSecret = "foundationallm-authorizationapi-entra-clientsecret";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The Entra ID scopes.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Entra_Scopes = "foundationallm-authorizationapi-entra-scopes";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The name of the storage account used by the Authorization API.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_Storage_AccountName = "foundationallm-authorizationapi-storage-accountname";

        /// <summary>
        /// The foundationallm-authorizationapi-entra- Key Vault secret name.
        /// The comma separated list of the identifiers of FoundationaLLM instances managed by the authorization core.
        /// </summary>
        public const string FoundationaLLM_AuthorizationAPI_InstanceIds = "foundationallm-authorizationapi-instanceids";

        #endregion
    }
}
