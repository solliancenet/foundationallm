namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Contains constants of the keys for all app configuration settings.
    /// </summary>
    public static class AppConfigurationKeys
    {
        /// <summary>
        /// The key for the FoundationaLLM:Instance:Id app configuration setting.
        /// The value should be a GUID represents a unique instance of the FoundationaLLM instance.
        /// </summary>
        public const string FoundationaLLM_Instance_Id = "FoundationaLLM:Instance:Id";
        /// <summary>
        /// Key for the FoundationaLLM:Configuration:KeyVaultURI app configuration setting.
        /// This value should be the URI of the Azure Key Vault that contains the application's secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultURI = "FoundationaLLM:Configuration:KeyVaultURI";
        /// <summary>
        /// Key for the FoundationaLLM:Configuration:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Configuration_ResourceProviderService_Storage_AuthenticationType =
            "FoundationaLLM:Configuration:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:AgentHub:AgentMetadata:StorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AgentHub_AgentMetadata_StorageContainer = "FoundationaLLM:AgentHub:AgentMetadata:StorageContainer";
        /// <summary>
        /// The key for the FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_AgentHub_StorageManager_BlobStorage_ConnectionString = "FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:Agent:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_Agent_ResourceProviderService_Storage_AuthenticationType = "FoundationaLLM:Agent:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Agent:ResourceProviderService:Storage:ConnectionString app configuration setting.
        /// The connection string to the Azure Storage account used for the agent resource provider.
        /// </summary>
        public const string FoundationaLLM_Agent_ResourceProviderService_Storage_ConnectionString = "FoundationaLLM:Agent:ResourceProviderService:Storage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:OrchestrationAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_OrchestrationAPI_APIKey = "FoundationaLLM:APIs:OrchestrationAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:OrchestrationAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_OrchestrationAPI_APIUrl = "FoundationaLLM:APIs:OrchestrationAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:OrchestrationAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_OrchestrationAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:OrchestrationAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentHubAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI_APIKey = "FoundationaLLM:APIs:AgentHubAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentHubAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI_APIUrl = "FoundationaLLM:APIs:AgentHubAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentHubAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:AgentHubAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AuthorizationAPI:APIScope app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_AuthorizationAPI_APIScope = "FoundationaLLM:APIs:AuthorizationAPI:APIScope";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AuthorizationAPI:APIUrl app configuration setting.
        /// The URL of the authorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_AuthorizationAPI_APIUrl = "FoundationaLLM:APIs:AuthorizationAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:CoreAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI_APIUrl = "FoundationaLLM:APIs:CoreAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:CoreAPI:BypassGatekeeper app configuration setting.
        /// By default, the Core API does not bypass the Gatekeeper API. To override this behavior and allow it to bypass the Gatekeeper API, set this value to true. Beware that bypassing the Gatekeeper means that you bypass content protection and filtering in favor of improved performance. Make sure you understand the risks before setting this value to true.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI_BypassGatekeeper = "FoundationaLLM:APIs:CoreAPI:BypassGatekeeper";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationAPI:APIUrl app configuration setting.
        /// The URL of the vectorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_APIUrl = "FoundationaLLM:APIs:VectorizationAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationAPI:APIKey app configuration setting.
        /// The API key of the vectorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_APIKey = "FoundationaLLM:APIs:VectorizationAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString app configuration setting.
        /// The connection string to the Application Insights instance used by the vectorization API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationWorker:APIUrl app configuration setting.
        /// The URL of the vectorization worker API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_APIUrl = "FoundationaLLM:APIs:VectorizationWorker:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationWorker:APIKey app configuration setting.
        /// The API key of the vectorization worker API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_APIKey = "FoundationaLLM:APIs:VectorizationWorker:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString app configuration setting.
        /// The connection string to the Application Insights instance used by the vectorization worker API.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_AppInsightsConnectionString = "FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatewayAPI:APIUrl app configuration setting.
        /// The URL of the gateway API.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI_APIUrl = "FoundationaLLM:APIs:GatewayAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatewayAPI:APIKey app configuration setting.
        /// The API key of the gateway API.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI_APIKey = "FoundationaLLM:APIs:GatewayAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatewayAPI:AppInsightsConnectionString app configuration setting.
        /// The connection string to the Application Insights instance used by the gateway API.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:GatewayAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:DataSource:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_DataSource_ResourceProviderService_Storage_AuthenticationType = "FoundationaLLM:DataSource:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:DataSource:ResourceProviderService:Storage:ConnectionString app configuration setting.
        /// The connection string to the Azure Storage account used for the data source resource provider.
        /// </summary>
        public const string FoundationaLLM_DataSource_ResourceProviderService_Storage_ConnectionString = "FoundationaLLM:DataSource:ResourceProviderService:Storage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:DataSourceHubAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_DataSourceHubAPI_APIKey = "FoundationaLLM:APIs:DataSourceHubAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:DataSourceHubAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_DataSourceHubAPI_APIUrl = "FoundationaLLM:APIs:DataSourceHubAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:DataSourceHubAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_DataSourceHubAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:DataSourceHubAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:Attachment:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_Attachment_ResourceProviderService_Storage_AuthenticationType = "FoundationaLLM:Attachment:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Attachment:ResourceProviderService:Storage:AccountName app configuration setting.
        /// This is the name of the storage account used when the authentication type is `AzureIdentity`, which is the default.
        /// </summary>
        public const string FoundationaLLM_Attachment_ResourceProviderService_Storage_AccountName =
            "FoundationaLLM:Attachment:ResourceProviderService:Storage:AccountName";
        /// <summary>
        /// The key for the FoundationaLLM:Attachment:ResourceProviderService:Storage:ConnectionString app configuration setting.
        /// The connection string to the Azure Storage account used for the data source resource provider.
        /// </summary>
        public const string FoundationaLLM_Attachment_ResourceProviderService_Storage_ConnectionString = "FoundationaLLM:Attachment:ResourceProviderService:Storage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_APIKey = "FoundationaLLM:APIs:GatekeeperAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_APIUrl = "FoundationaLLM:APIs:GatekeeperAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafety app configuration setting.
        /// By default, the Gatekeeper API has Azure Content Safety integration enabled. To disable this feature, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableAzureContentSafety = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafety";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableLakeraGuard app configuration setting.
        /// By default, the Gatekeeper API has Lakera Guard integration disabled. To enable this feature, set this value to true.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableLakeraGuard = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableLakeraGuard";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableEnkryptGuardrails app configuration setting.
        /// By default, the Gatekeeper API has Lakera Guard integration disabled. To enable this feature, set this value to true.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableEnkryptGuardrails = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableEnkryptGuardrails";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShield app configuration setting.
        /// By default, the Gatekeeper API has Azure Content Safety Prompt Shield integration enabled. To disable this feature, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableAzureContentSafetyPromptShield = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShield";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio app configuration setting.
        /// By default, the Gatekeeper API has Microsoft Presidio integration enabled. To disable this feature, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableMicrosoftPresidio = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:Gatekeeper:LakeraGuard app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_LakeraGuard = "FoundationaLLM:APIs:Gatekeeper:LakeraGuard";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_EnkryptGuardrails = "FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey = "FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIUrl = "FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:LangChainAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI_APIKey = "FoundationaLLM:APIs:LangChainAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:LangChainAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI_APIUrl = "FoundationaLLM:APIs:LangChainAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:LangChainAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:LangChainAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:ManagementAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_ManagementAPI_APIUrl = "FoundationaLLM:APIs:ManagementAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:ManagementAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_ManagementAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:ManagementAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:Prompt:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_Prompt_ResourceProviderService_Storage_AuthenticationType = "FoundationaLLM:Prompt:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Prompt:ResourceProviderService:Storage:ConnectionString app configuration setting.
        /// The connection string to the Azure Storage account used for the prompt resource provider.
        /// </summary>
        public const string FoundationaLLM_Prompt_ResourceProviderService_Storage_ConnectionString = "FoundationaLLM:Prompt:ResourceProviderService:Storage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:PromptHubAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI_APIKey = "FoundationaLLM:APIs:PromptHubAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:PromptHubAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI_APIUrl = "FoundationaLLM:APIs:PromptHubAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:PromptHubAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:PromptHubAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:SemanticKernelAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI_APIKey = "FoundationaLLM:APIs:SemanticKernelAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:SemanticKernelAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI_APIUrl = "FoundationaLLM:APIs:SemanticKernelAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_APIKey = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_APIUrl = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:HateSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_HateSeverity = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:HateSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SelfHarmSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_SelfHarmSeverity = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SelfHarmSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SexualSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_SexualSeverity = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SexualSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:ViolenceSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety_ViolenceSeverity = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:ViolenceSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_LakeraGuard_APIKey = "FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_LakeraGuard_APIUrl = "FoundationaLLM:APIs:Gatekeeper:LakeraGuard:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_EnkryptGuardrails_APIKey = "FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_EnkryptGuardrails_APIUrl = "FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Completions_DeploymentName = "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Completions_MaxTokens = "FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Completions:ModelName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Completions_ModelName = "FoundationaLLM:AzureOpenAI:API:Completions:ModelName";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Completions_ModelVersion = "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Completions:Temperature app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Completions_Temperature = "FoundationaLLM:AzureOpenAI:API:Completions:Temperature";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Embeddings_DeploymentName = "FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Embeddings:MaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Embeddings_MaxTokens = "FoundationaLLM:AzureOpenAI:API:Embeddings:MaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Embeddings_ModelName = "FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Embeddings:Temperature app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Embeddings_Temperature = "FoundationaLLM:AzureOpenAI:API:Embeddings:Temperature";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Endpoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Endpoint = "FoundationaLLM:AzureOpenAI:API:Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Key = "FoundationaLLM:AzureOpenAI:API:Key";
        /// <summary>
        /// The key for the FoundationaLLM:AzureOpenAI:API:Version app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_API_Version = "FoundationaLLM:AzureOpenAI:API:Version";
        /// <summary>
        /// The key for the FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource_BlobStorageConnection = "FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection";
        /// <summary>
        /// The key for the FoundationaLLM:BlobStorageMemorySource:BlobStorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource_BlobStorageContainer = "FoundationaLLM:BlobStorageMemorySource:BlobStorageContainer";
        /// <summary>
        /// The key for the FoundationaLLM:BlobStorageMemorySource:ConfigFilePath app configuration setting.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource_ConfigFilePath = "FoundationaLLM:BlobStorageMemorySource:ConfigFilePath";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:AccentColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_AccentColor = "FoundationaLLM:Branding:AccentColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:AccentTextColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_AccentTextColor = "FoundationaLLM:Branding:AccentTextColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:BackgroundColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_BackgroundColor = "FoundationaLLM:Branding:BackgroundColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:CompanyName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_CompanyName = "FoundationaLLM:Branding:CompanyName";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:FavIconUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_FavIconUrl = "FoundationaLLM:Branding:FavIconUrl";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:KioskMode app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_KioskMode = "FoundationaLLM:Branding:KioskMode";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:LogoText app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_LogoText = "FoundationaLLM:Branding:LogoText";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:LogoUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_LogoUrl = "FoundationaLLM:Branding:LogoUrl";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:PageTitle app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_PageTitle = "FoundationaLLM:Branding:PageTitle";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:PrimaryColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryColor = "FoundationaLLM:Branding:PrimaryColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:PrimaryTextColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryTextColor = "FoundationaLLM:Branding:PrimaryTextColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:SecondaryColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryColor = "FoundationaLLM:Branding:SecondaryColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:SecondaryTextColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryTextColor = "FoundationaLLM:Branding:SecondaryTextColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:PrimaryButtonBackgroundColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryButtonBackgroundColor = "FoundationaLLM:Branding:PrimaryButtonBackgroundColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:PrimaryButtonTextColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryButtonTextColor = "FoundationaLLM:Branding:PrimaryButtonTextColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:SecondaryButtonBackgroundColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryButtonBackgroundColor = "FoundationaLLM:Branding:SecondaryButtonBackgroundColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:SecondaryButtonTextColor app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryButtonTextColor = "FoundationaLLM:Branding:SecondaryButtonTextColor";
        /// <summary>
        /// The key for the FoundationaLLM:Branding:FooterText app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Branding_FooterText = "FoundationaLLM:Branding:FooterText";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:CallbackPath app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_CallbackPath = "FoundationaLLM:Chat:Entra:CallbackPath";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:ClientId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_ClientId = "FoundationaLLM:Chat:Entra:ClientId";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:ClientSecret app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_ClientSecret = "FoundationaLLM:Chat:Entra:ClientSecret";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:Instance app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_Instance = "FoundationaLLM:Chat:Entra:Instance";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:Scopes app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_Scopes = "FoundationaLLM:Chat:Entra:Scopes";
        /// <summary>
        /// The key for the FoundationaLLM:Chat:Entra:TenantId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra_TenantId = "FoundationaLLM:Chat:Entra:TenantId";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:CallbackPath app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_CallbackPath = "FoundationaLLM:CoreAPI:Entra:CallbackPath";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:ClientId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_ClientId = "FoundationaLLM:CoreAPI:Entra:ClientId";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:ClientSecret app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_ClientSecret = "FoundationaLLM:CoreAPI:Entra:ClientSecret";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:Instance app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_Instance = "FoundationaLLM:CoreAPI:Entra:Instance";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:Scopes app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_Scopes = "FoundationaLLM:CoreAPI:Entra:Scopes";
        /// <summary>
        /// The key for the FoundationaLLM:CoreAPI:Entra:TenantId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra_TenantId = "FoundationaLLM:CoreAPI:Entra:TenantId";
        /// <summary>
        /// The key for the FoundationaLLM:CoreWorker:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CoreWorker_AppInsightsConnectionString = "FoundationaLLM:CoreWorker:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_ChangeFeedLeaseContainer = "FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:Containers app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_Containers = "FoundationaLLM:CosmosDB:Containers";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:Database app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_Database = "FoundationaLLM:CosmosDB:Database";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:Endpoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_Endpoint = "FoundationaLLM:CosmosDB:Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_Key = "FoundationaLLM:CosmosDB:Key";
        /// <summary>
        /// The key for the FoundationaLLM:CosmosDB:MonitoredContainers app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CosmosDB_MonitoredContainers = "FoundationaLLM:CosmosDB:MonitoredContainers";
        /// <summary>
        /// The key for the FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_DataSourceHub_DataSourceMetadata_StorageContainer = "FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer";
        /// <summary>
        /// The key for the FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_DataSourceHub_StorageManager_BlobStorage_ConnectionString = "FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:DataSources:AboutFoundationaLLM:BlobStorage:ConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_DataSources_AboutFoundationaLLM_BlobStorage_ConnectionString = "FoundationaLLM:DataSources:AboutFoundationaLLM:BlobStorage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:LangChain:CSVFile:URL app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_LangChain_CSVFile_URL = "FoundationaLLM:LangChain:CSVFile:URL";
        /// <summary>
        /// The key for the FoundationaLLM:LangChain:SQLDatabase:TestDB:Password app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_LangChain_SQLDatabase_TestDB_Password = "FoundationaLLM:LangChain:SQLDatabase:TestDB:Password";
        /// <summary>
        /// The key for the FoundationaLLM:LangChain:Summary:MaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_LangChain_Summary_MaxTokens = "FoundationaLLM:LangChain:Summary:MaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:LangChain:Summary:ModelName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_LangChain_Summary_ModelName = "FoundationaLLM:LangChain:Summary:ModelName";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:CallbackPath app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_CallbackPath = "FoundationaLLM:Management:Entra:CallbackPath";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:ClientId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_ClientId = "FoundationaLLM:Management:Entra:ClientId";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:ClientSecret app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_ClientSecret = "FoundationaLLM:Management:Entra:ClientSecret";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:Instance app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_Instance = "FoundationaLLM:Management:Entra:Instance";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:Scopes app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_Scopes = "FoundationaLLM:Management:Entra:Scopes";
        /// <summary>
        /// The key for the FoundationaLLM:Management:Entra:TenantId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Management_Entra_TenantId = "FoundationaLLM:Management:Entra:TenantId";
        /// <summary>
        /// The key for the FoundationaLLM:ManagementAPI:Entra:ClientId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra_ClientId = "FoundationaLLM:ManagementAPI:Entra:ClientId";
        /// <summary>
        /// The key for the FoundationaLLM:ManagementAPI:Entra:ClientSecret app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra_ClientSecret = "FoundationaLLM:ManagementAPI:Entra:ClientSecret";
        /// <summary>
        /// The key for the FoundationaLLM:ManagementAPI:Entra:Instance app configuration setting.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra_Instance = "FoundationaLLM:ManagementAPI:Entra:Instance";
        /// <summary>
        /// The key for the FoundationaLLM:ManagementAPI:Entra:Scopes app configuration setting.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra_Scopes = "FoundationaLLM:ManagementAPI:Entra:Scopes";
        /// <summary>
        /// The key for the FoundationaLLM:ManagementAPI:Entra:TenantId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra_TenantId = "FoundationaLLM:ManagementAPI:Entra:TenantId";
        /// <summary>
        /// The key for the FoundationaLLM:OpenAI:API:Endpoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_OpenAI_API_Endpoint = "FoundationaLLM:OpenAI:API:Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:OpenAI:API:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_OpenAI_API_Key = "FoundationaLLM:OpenAI:API:Key";
        /// <summary>
        /// The key for the FoundationaLLM:OpenAI:API:Temperature app configuration setting.
        /// </summary>
        public const string FoundationaLLM_OpenAI_API_Temperature = "FoundationaLLM:OpenAI:API:Temperature";
        /// <summary>
        /// The key for the FoundationaLLM:PromptHub:PromptMetadata:StorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_PromptHub_PromptMetadata_StorageContainer = "FoundationaLLM:PromptHub:PromptMetadata:StorageContainer";
        /// <summary>
        /// The key for the FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_PromptHub_StorageManager_BlobStorage_ConnectionString = "FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_Key = "FoundationaLLM:SemanticKernelAPI:OpenAI:Key";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.ChatCompletionPromptName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_ChatCompletionPromptName = "FoundationaLLM:SemanticKernelAPI:OpenAI.ChatCompletionPromptName";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeployment app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_CompletionsDeployment = "FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeployment";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeploymentMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_CompletionsDeploymentMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeploymentMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeployment app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_EmbeddingsDeployment = "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeployment";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_EmbeddingsDeploymentMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.Endpoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_Endpoint = "FoundationaLLM:SemanticKernelAPI:OpenAI.Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_CompletionsMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMinTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_CompletionsMinTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMinTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_MemoryMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMinTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_MemoryMinTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMinTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_MessagesMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMinTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_MessagesMinTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMinTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.SystemMaxTokens app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_PromptOptimization_SystemMaxTokens = "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.SystemMaxTokens";
        /// <summary>
        /// The key for the FoundationaLLM:SemanticKernelAPI:OpenAI.ShortSummaryPromptName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI_OpenAI_ShortSummaryPromptName = "FoundationaLLM:SemanticKernelAPI:OpenAI.ShortSummaryPromptName";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:VectorizationWorker app configuration setting.
        /// The settings used by each instance of the vectorization worker service. For more details, see [default vectorization worker settings](../setup-guides/vectorization/vectorization-worker.md#default-vectorization-worker-settings)
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationWorker = "FoundationaLLM:Vectorization:VectorizationWorker";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:Queues:Embed:AccountName app configuration setting.
        /// The name of the Azure Storage account used for the embed vectorization queue.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Embed_AccountName = "FoundationaLLM:Vectorization:Queues:Embed:AccountName";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:Queues:Extract:AccountName app configuration setting.
        /// The name of the Azure Storage account used for the extract vectorization queue.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Extract_AccountName = "FoundationaLLM:Vectorization:Queues:Extract:AccountName";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:Queues:Index:AccountName app configuration setting.
        /// The name of the Azure Storage account used for the index vectorization queue.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Index_AccountName = "FoundationaLLM:Vectorization:Queues:Index:AccountName";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:Queues:Partition:AccountName app configuration setting.
        /// The name of the Azure Storage account used for the partition vectorization queue.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Partition_AccountName = "FoundationaLLM:Vectorization:Queues:Partition:AccountName";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_Vectorization_StateService_Storage_AuthenticationType = "FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProviderService_Storage_AuthenticationType = "FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString app configuration setting.
        /// The connection string to the Azure Storage account used for the vectorization state service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProviderService_Storage_ConnectionString = "FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType app configuration setting.
        /// The authentication type used to connect to the Azure AI Search service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureAISearchIndexingService_AuthenticationType = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint app configuration setting.
        /// The endpoint of the Azure AI Search service.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureAISearchIndexingService_Endpoint = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_APIKey = "FoundationaLLM:Events:AzureEventGridEventService:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:AuthenticationType app configuration setting.
        /// Default value is 'APIKey'.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_AuthenticationType = "FoundationaLLM:Events:AzureEventGridEventService:AuthenticationType";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Endpoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Endpoint = "FoundationaLLM:Events:AzureEventGridEventService:Endpoint";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:NamespaceId app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_NamespaceId = "FoundationaLLM:Events:AzureEventGridEventService:NamespaceId";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:CoreAPI app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_CoreAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:CoreAPI";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_OrchestrationAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:ManagementAPI app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_ManagementAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:ManagementAPI";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationAPI app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationAPI";
        /// <summary>
        /// The key for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationWorker app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationWorker = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationWorker";
        /// <summary>
        /// The key for the FoundationaLLM:APIEndpoints:AzureContentSafety01:APIKey app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety01_APIKey = "FoundationaLLM:APIEndpoints:AzureContentSafety01:APIKey";
    }

    /// <summary>
    /// Contains constants of the keys filters for app configuration setting namespaces.
    /// </summary>
    public static class AppConfigurationKeyFilters
    {
        /// <summary>
        /// The key filter for the FoundationaLLM:Instance:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Instance = "FoundationaLLM:Instance:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Configuration:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Configuration = "FoundationaLLM:Configuration:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Branding:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Branding = "FoundationaLLM:Branding:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs = "FoundationaLLM:APIs:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:ExternalAPIs:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_ExternalAPIs = "FoundationaLLM:ExternalAPIs:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:VectorizationAPI:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI = "FoundationaLLM:APIs:VectorizationAPI:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:VectorizationWorker:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker = "FoundationaLLM:APIs:VectorizationWorker:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:GatewayAPI:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI = "FoundationaLLM:APIs:GatewayAPI:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:CosmosDB:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CosmosDB = "FoundationaLLM:CosmosDB:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:CoreAPI:Entra:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CoreAPI_Entra = "FoundationaLLM:CoreAPI:Entra:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:ManagementAPI:Entra:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_ManagementAPI_Entra = "FoundationaLLM:ManagementAPI:Entra:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Chat:Entra:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Chat_Entra = "FoundationaLLM:Chat:Entra:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Management:Entra:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Management_Entra = "FoundationaLLM:Management:Entra:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Orchestration:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Orchestration = "FoundationaLLM:Orchestration:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:CoreWorker:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CoreWorker = "FoundationaLLM:CoreWorker:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Refinement:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Refinement = "FoundationaLLM:Refinement:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:AzureContentSafety:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety = "FoundationaLLM:AzureContentSafety:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:BlobStorageMemorySource:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource = "FoundationaLLM:CoreAPI:BlobStorageMemorySource:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Vectorization:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization = "FoundationaLLM:Vectorization:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Agent:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Agent = "FoundationaLLM:Agent:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Prompt:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Prompt = "FoundationaLLM:Prompt:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Events:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events = "FoundationaLLM:Events:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:DataSource:* app configuration settings.
        /// This supports the DataSource resource provider settings.
        /// </summary>
        public const string FoundationaLLM_DataSource = "FoundationaLLM:DataSource:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:DataSources:* app configuration settings.
        /// This supports data source settings created by the Management API.
        /// </summary>
        public const string FoundationaLLM_DataSources = "FoundationaLLM:DataSources:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Attachment:* app configuration settings.
        /// This supports the Attachment resource provider settings.
        /// </summary>
        public const string FoundationaLLM_Attachment = "FoundationaLLM:Attachment:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:AzureOpenAI:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI = "FoundationaLLM:AzureOpenAI:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:AzureAI:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureAI = "FoundationaLLM:AzureAI:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Gateway:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Gateway = "FoundationaLLM:Gateway:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:AzureAIStudio:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureAIStudio = "FoundationaLLM:AzureAIStudio:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:AzureAIStudio:BlobStorageServiceSettings:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureAIStudio_BlobStorageServiceSettings = "FoundationaLLM:AzureAIStudio:BlobStorageServiceSettings:*";
    }

    /// <summary>
    /// Contains constants of the keys sections for app configuration setting namespaces.
    /// </summary>
    public static class AppConfigurationKeySections
    {
        /// <summary>
        /// The key section for the FoundationaLLM:Instance app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Instance = "FoundationaLLM:Instance";
        /// <summary>
        /// The key section for the FoundationaLLM:Branding app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Branding = "FoundationaLLM:Branding";
        /// <summary>
        /// The key section for the FoundationaLLM:CosmosDB app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CosmosDB = "FoundationaLLM:CosmosDB";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:CoreAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI = "FoundationaLLM:APIs:CoreAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:OrchestrationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_OrchestrationAPI = "FoundationaLLM:APIs:OrchestrationAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:SemanticKernelAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_SemanticKernelAPI = "FoundationaLLM:APIs:SemanticKernelAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:LangChainAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_LangChainAPI = "FoundationaLLM:APIs:LangChainAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:AgentHubAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentHubAPI = "FoundationaLLM:APIs:AgentHubAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:OrchestrationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_PromptHubAPI = "FoundationaLLM:APIs:PromptHubAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:GatekeeperAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI = "FoundationaLLM:APIs:GatekeeperAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:GatekeeperAPI:Configuration app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration = "FoundationaLLM:APIs:GatekeeperAPI:Configuration";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:VectorizationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI = "FoundationaLLM:APIs:VectorizationAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:VectorizationWorker app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker = "FoundationaLLM:APIs:VectorizationWorker";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:AuthorizationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_AuthorizationAPI = "FoundationaLLM:APIs:AuthorizationAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:APIs:GatewayAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_GatewayAPI = "FoundationaLLM:APIs:GatewayAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:ExternalAPIs app configuration settings.
        /// </summary>
        public const string FoundationaLLM_ExternalAPIs = "FoundationaLLM:ExternalAPIs";
        /// <summary>
        /// The key section for the FoundationaLLM:Orchestration app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Orchestration = "FoundationaLLM:Orchestration";
        /// <summary>
        /// The key section for the FoundationaLLM:SemanticKernelAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_SemanticKernelAPI = "FoundationaLLM:SemanticKernelAPI";
        /// <summary>
        /// The key section for the FoundationaLLM:Refinement app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Refinement = "FoundationaLLM:Refinement";
        /// <summary>
        /// The key section for the FoundationaLLM:AzureContentSafety app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_AzureContentSafety = "FoundationaLLM:APIs:Gatekeeper:AzureContentSafety";
        /// <summary>
        /// The key section for the FoundationaLLM:EnkryptGuardrails app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_EnkryptGuardrails = "FoundationaLLM:APIs:Gatekeeper:EnkryptGuardrails";
        /// <summary>
        /// The key section for the FoundationaLLM:LakeraGuard app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_Gatekeeper_LakeraGuard = "FoundationaLLM:APIs:Gatekeeper:LakeraGuard";
        /// <summary>
        /// The key section for the FoundationaLLM:BlobStorageMemorySource app configuration settings.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource = "FoundationaLLM:BlobStorageMemorySource";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:Steps app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Steps = "FoundationaLLM:Vectorization:Steps";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:Queues app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues = "FoundationaLLM:Vectorization:Queues";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:StateService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_StateService = "FoundationaLLM:Vectorization:StateService:Storage";
        /// <summary>
        /// The key section for the FoundationaLLM:DataSources app configuration settings.
        /// </summary>
        public const string FoundationaLLM_DataSources = "FoundationaLLM:DataSources";
        /// <summary>
        /// The key section for the FoundationaLLM:Attachments app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Attachments = "FoundationaLLM:Attachments";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:AzureAISearchIndexingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureAISearchIndexingService = "FoundationaLLM:Vectorization:AzureAISearchIndexingService";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:AzureCosmosDBNoSQLIndexingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureCosmosDBNoSQLIndexingService = "FoundationaLLM:Vectorization:AzureCosmosDBNoSQLIndexingService";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:PostgresIndexingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_PostgresIndexingService = "FoundationaLLM:Vectorization:PostgresIndexingService";
        /// <summary>
        /// The key section for the FoundationaLLM:Gateway app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Gateway = "FoundationaLLM:Gateway";
        /// <summary>
        /// The key section for the FoundationaLLM:AzureAIStudio app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureAIStudio = "FoundationaLLM:AzureAIStudio";
        /// <summary>
        /// The key section for the FoundationaLLM:AzureAIStudio:BlobStorageServiceSettings app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AzureAIStudio_BlobStorageServiceSettings = "FoundationaLLM:AzureAIStudio:BlobStorageServiceSettings";

        #region Resource providers

        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProviderService_Storage = "FoundationaLLM:Vectorization:ResourceProviderService:Storage";

        /// <summary>
        /// The key section for the FoundationaLLM:Agent:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Agent_ResourceProviderService_Storage = "FoundationaLLM:Agent:ResourceProviderService:Storage";

        /// <summary>
        /// The key section for the FoundationaLLM:Prompt:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Prompt_ResourceProviderService_Storage = "FoundationaLLM:Prompt:ResourceProviderService:Storage";

        /// <summary>
        /// The key section for the FoundationaLLM:Configuration:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Configuration_ResourceProviderService_Storage = "FoundationaLLM:Configuration:ResourceProviderService:Storage";

        /// <summary>
        /// The key section for the FoundationaLLM:DataSource:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_DataSource_ResourceProviderService_Storage = "FoundationaLLM:DataSource:ResourceProviderService:Storage";

        /// <summary>
        /// The key section for the FoundationaLLM:Attachment:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Attachment_ResourceProviderService_Storage = "FoundationaLLM:Attachment:ResourceProviderService:Storage";
        #endregion

        #region Event Grid events

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService = "FoundationaLLM:Events:AzureEventGridEventService";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:CoreAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_CoreAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:CoreAPI";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_OrchestrationAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:OrchestrationAPI";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:SemanticKernelAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_SemanticKernelAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:SemanticKernelAPI";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:ManagementAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_ManagementAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:ManagementAPI";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationAPI = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationAPI";

        /// <summary>
        /// The key section for the FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationWorker app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Events_AzureEventGridEventService_Profiles_VectorizationWorker = "FoundationaLLM:Events:AzureEventGridEventService:Profiles:VectorizationWorker";

        #endregion
    }
}
