// -------------------------------------------------------------------------------
//
// WARNING!
// This file is auto-generated based on the AppConfiguration.json file.
// Do not make changes to this file, as they will be automatically overwritten.
//
// -------------------------------------------------------------------------------
namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Defines all App Configuration key names used by FoundationaLLM.
    /// </summary>
    public static class AppConfigurationKeys
    {
        #region FoundationaLLM:Instance
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Instance:Id setting.
        /// <para>Value description:<br/>The unique identifier of the FoundationaLLM instance.</para>
        /// </summary>
        public const string FoundationaLLM_Instance_Id =
            "FoundationaLLM:Instance:Id";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Instance:SecurityGroupRetrievalStrategy setting.
        /// <para>Value description:<br/>The security group retrieval strategy of the FoundationaLLM instance.</para>
        /// </summary>
        public const string FoundationaLLM_Instance_SecurityGroupRetrievalStrategy =
            "FoundationaLLM:Instance:SecurityGroupRetrievalStrategy";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId setting.
        /// <para>Value description:<br/>The object identifier of the security principal who is allowed to substitute its identity with a value provided in the X-USER-IDENTITY header.</para>
        /// </summary>
        public const string FoundationaLLM_Instance_IdentitySubstitutionSecurityPrincipalId =
            "FoundationaLLM:Instance:IdentitySubstitutionSecurityPrincipalId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern setting.
        /// <para>Value description:<br/>The Regex pattern used to validate the values allowed as User Principal Name (UPN) substitutes in the X-USER-IDENTITY header.</para>
        /// </summary>
        public const string FoundationaLLM_Instance_IdentitySubstitutionUserPrincipalNamePattern =
            "FoundationaLLM:Instance:IdentitySubstitutionUserPrincipalNamePattern";

        #endregion

        #region FoundationaLLM:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Configuration:KeyVaultURI setting.
        /// <para>Value description:<br/>The URL of the Azure Key Vault providing the secrets.</para>
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultURI =
            "FoundationaLLM:Configuration:KeyVaultURI";

        #endregion

        #region FoundationaLLM:PythonSDK
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:PythonSDK:Logging:LogLevel:Default setting.
        /// <para>Value description:<br/>The default logging level used by the Python SDK.</para>
        /// </summary>
        public const string FoundationaLLM_PythonSDK_Logging_LogLevel_Default =
            "FoundationaLLM:PythonSDK:Logging:LogLevel:Default";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:PythonSDK:Logging:LogLevel:Azure setting.
        /// <para>Value description:<br/>The default logging level used by the Python SDK.</para>
        /// </summary>
        public const string FoundationaLLM_PythonSDK_Logging_LogLevel_Azure =
            "FoundationaLLM:PythonSDK:Logging:LogLevel:Azure";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:PythonSDK:Logging:EnableConsoleLogging setting.
        /// <para>Value description:<br/>The flag indicating whether the Python SDK sends logs to the console or not.</para>
        /// </summary>
        public const string FoundationaLLM_PythonSDK_Logging_EnableConsoleLogging =
            "FoundationaLLM:PythonSDK:Logging:EnableConsoleLogging";

        #endregion

        #region FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CodeInterpreter setting.
        /// <para>Value description:<br/>The settings for the Azure Container Apps Dynamic Sessions code interpreter code execution service.</para>
        /// </summary>
        public const string FoundationaLLM_Code_CodeExecution_AzureContainerAppsDynamicSessions_CodeInterpreter =
            "FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CodeInterpreter";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CustomContainer setting.
        /// <para>Value description:<br/>The settings for the Azure Container Apps Dynamic Sessions custom container code execution service.</para>
        /// </summary>
        public const string FoundationaLLM_Code_CodeExecution_AzureContainerAppsDynamicSessions_CustomContainer =
            "FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CustomContainer";

        #endregion

        #region FoundationaLLM:ResourceProvidersCache
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProvidersCache:EnableCache setting.
        /// <para>Value description:<br/>Setting that indicates whether the resource providers should use caching.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProvidersCache_EnableCache =
            "FoundationaLLM:ResourceProvidersCache:EnableCache";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProvidersCache:AbsoluteCacheExpirationSeconds setting.
        /// <para>Value description:<br/>Absolute cache expiration in seconds.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProvidersCache_AbsoluteCacheExpirationSeconds =
            "FoundationaLLM:ResourceProvidersCache:AbsoluteCacheExpirationSeconds";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProvidersCache:SlidingCacheExpirationSeconds setting.
        /// <para>Value description:<br/>Sets how many seconds the cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set).</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProvidersCache_SlidingCacheExpirationSeconds =
            "FoundationaLLM:ResourceProvidersCache:SlidingCacheExpirationSeconds";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProvidersCache:CacheSizeLimit setting.
        /// <para>Value description:<br/>The maximum number of items that can be stored in the cache.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProvidersCache_CacheSizeLimit =
            "FoundationaLLM:ResourceProvidersCache:CacheSizeLimit";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProvidersCache:CacheExpirationScanFrequencySeconds setting.
        /// <para>Value description:<br/>Gets or sets the minimum length of time between successive scans for expired items in seconds.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProvidersCache_CacheExpirationScanFrequencySeconds =
            "FoundationaLLM:ResourceProvidersCache:CacheExpirationScanFrequencySeconds";

        #endregion

        #region FoundationaLLM:Quota

        #endregion

        #region FoundationaLLM:Quota:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Quota:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM quota management service. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_Quota_Storage_AuthenticationType =
            "FoundationaLLM:Quota:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Quota:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM quota management service.</para>
        /// </summary>
        public const string FoundationaLLM_Quota_Storage_AccountName =
            "FoundationaLLM:Quota:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:AIModel

        #endregion

        #region FoundationaLLM:ResourceProviders:AIModel:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:AIModel:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.AIModel resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_AIModel_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:AIModel:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:AIModel:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.AIModel resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_AIModel_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:AIModel:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Agent

        #endregion

        #region FoundationaLLM:ResourceProviders:Agent:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Agent:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Agent resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Agent_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Agent:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Agent:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Agent resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Agent_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Agent:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Attachment

        #endregion

        #region FoundationaLLM:ResourceProviders:Attachment:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Attachment resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Attachment_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Attachment resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Attachment_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Configuration

        #endregion

        #region FoundationaLLM:ResourceProviders:Configuration:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Configuration:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Configuration resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Configuration_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Configuration:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Configuration:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Configuration resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Configuration_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Configuration:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:DataSource

        #endregion

        #region FoundationaLLM:ResourceProviders:DataSource:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:DataSource:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.DataSource resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_DataSource_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:DataSource:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:DataSource:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.DataSource resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_DataSource_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:DataSource:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Prompt

        #endregion

        #region FoundationaLLM:ResourceProviders:Prompt:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Prompt:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Prompt resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Prompt_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Prompt:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Prompt:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Prompt resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Prompt_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Prompt:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Vectorization

        #endregion

        #region FoundationaLLM:ResourceProviders:Vectorization:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Vectorization:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Vectorization resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Vectorization_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Vectorization:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Vectorization:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Vectorization resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Vectorization_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Vectorization:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:DataPipeline

        #endregion

        #region FoundationaLLM:ResourceProviders:DataPipeline:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:DataPipeline:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.DataPipeline resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_DataPipeline_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:DataPipeline:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:DataPipeline:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.DataPipeline resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_DataPipeline_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:DataPipeline:Storage:AccountName";

        #endregion

        #region FoundationaLLM:ResourceProviders:Plugin

        #endregion

        #region FoundationaLLM:ResourceProviders:Plugin:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Plugin:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the FoundationaLLM.Plugin resource provider. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Plugin_Storage_AuthenticationType =
            "FoundationaLLM:ResourceProviders:Plugin:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ResourceProviders:Plugin:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the FoundationaLLM.Plugin resource provider.</para>
        /// </summary>
        public const string FoundationaLLM_ResourceProviders_Plugin_Storage_AccountName =
            "FoundationaLLM:ResourceProviders:Plugin:Storage:AccountName";

        #endregion

        #region FoundationaLLM:APIEndpoints

        #endregion

        #region FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIUrl setting.
        /// <para>Value description:<br/>The URL of the Authorization API. Due to its special nature, the Authorization API does not have a corresponding APIEndpointConfiguration resource and thus the URL must be kept as a configuration setting.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_APIUrl =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIScope setting.
        /// <para>Value description:<br/>The scope required to get an access token for the Authorization API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_APIScope =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:APIScope";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:EnableCache setting.
        /// <para>Value description:<br/>Setting that indicates whether the Authorization API Client should use caching.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_EnableCache =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:EnableCache";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:AbsoluteCacheExpirationSeconds setting.
        /// <para>Value description:<br/>Absolute cache expiration in seconds.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_AbsoluteCacheExpirationSeconds =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:AbsoluteCacheExpirationSeconds";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:SlidingCacheExpirationSeconds setting.
        /// <para>Value description:<br/>Sets how many seconds the cache entry can be inactive (e.g. not accessed) before it will be removed. This will not extend the entry lifetime beyond the absolute expiration (if set).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_SlidingCacheExpirationSeconds =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:SlidingCacheExpirationSeconds";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheSizeLimit setting.
        /// <para>Value description:<br/>The maximum number of items that can be stored in the cache.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_CacheSizeLimit =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheSizeLimit";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheExpirationScanFrequencySeconds setting.
        /// <para>Value description:<br/>Gets or sets the minimum length of time between successive scans for expired items in seconds.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials_CacheExpirationScanFrequencySeconds =
            "FoundationaLLM:APIEndpoints:AuthorizationAPI:Essentials:CacheExpirationScanFrequencySeconds";

        #endregion

        #region FoundationaLLM:APIEndpoints:CoreAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl setting.
        /// <para>Value description:<br/>The URL of the Core API. Since it's an entry point API, the Core API does not have a corresponding APIEndpointConfiguration resource and thus the URL must be kept as a configuration setting.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl =
            "FoundationaLLM:APIEndpoints:CoreAPI:Essentials:APIUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:CoreAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:CoreAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:BypassGatekeeper setting.
        /// <para>Value description:<br/>The flag that indicates whether the Core API should bypass or not the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_BypassGatekeeper =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:BypassGatekeeper";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsFileSearchFileExtensions setting.
        /// <para>Value description:<br/>The comma-separated list file extensions that are supported by the Azure OpenAI Assistants file search tool.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_AzureOpenAIAssistantsFileSearchFileExtensions =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureOpenAIAssistantsFileSearchFileExtensions";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureAIAgentsFileSearchFileExtensions setting.
        /// <para>Value description:<br/>The comma-separated list file extensions that are supported by the Azure AI Agent Service file search tool.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_AzureAIAgentsFileSearchFileExtensions =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AzureAIAgentsFileSearchFileExtensions";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions setting.
        /// <para>Value description:<br/>The comma-separated list file extensions that users are allowed to upload to a conversation.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_AllowedUploadFileExtensions =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:AllowedUploadFileExtensions";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:MaxUploadsPerMessage setting.
        /// <para>Value description:<br/>The maximum number of files that can be uploaded for a single conversation message.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_MaxUploadsPerMessage =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:MaxUploadsPerMessage";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalSeconds setting.
        /// <para>Value description:<br/>The size in seconds of the polling interval used to check for completion responses.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CompletionResponsePollingIntervalSeconds =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CompletionResponsePollingIntervalSeconds";

        #endregion

        #region FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:Instance setting.
        /// <para>Value description:<br/>The URL of the Entra ID instance used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_Instance =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:Instance";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:TenantId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID tenant used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_TenantId =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:TenantId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:Scopes setting.
        /// <para>Value description:<br/>The list of scopes exposed by the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_Scopes =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:Scopes";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:CallbackPath setting.
        /// <para>Value description:<br/>The path where the Entra ID instance will redirect during authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_CallbackPath =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:CallbackPath";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:ClientId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID app registration used by the Core API to authenticate.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_ClientId =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:ClientId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:RequireScopes setting.
        /// <para>Value description:<br/>Indicates whether a scope ("scp") claim is required for authorization.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_RequireScopes =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:RequireScopes";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:AllowACLAuthorization setting.
        /// <para>Value description:<br/>Indicates whether tokens that do not have either of the "scp" or "roles" claims are accepted (true means they are accepted).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_Entra_AllowACLAuthorization =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:Entra:AllowACLAuthorization";

        #endregion

        #region FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Endpoint setting.
        /// <para>Value description:<br/>The URL of the Azure Cosmos DB service used by the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB_Endpoint =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Endpoint";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Database setting.
        /// <para>Value description:<br/>The name of the Azure Cosmos DB database used by the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB_Database =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Database";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Containers setting.
        /// <para>Value description:<br/>The comma-separated list of database containers used by the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB_Containers =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:Containers";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:MonitoredContainers setting.
        /// <para>Value description:<br/>The comma-separated list of database containers that are monitored for changes using the change feed.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB_MonitoredContainers =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:MonitoredContainers";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:ChangeFeedLeaseContainer setting.
        /// <para>Value description:<br/>The name of the container used by Azure Cosmos DB to manage the change feed leases.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB_ChangeFeedLeaseContainer =
            "FoundationaLLM:APIEndpoints:CoreAPI:Configuration:CosmosDB:ChangeFeedLeaseContainer";

        #endregion

        #region FoundationaLLM:APIEndpoints:CoreWorker:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreWorker:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Core Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreWorker_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:CoreWorker:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:CoreWorker:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Core Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_CoreWorker_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:CoreWorker:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableLakeraGuard setting.
        /// <para>Value description:<br/>Indicates whether Lakera Guard is available for use by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Configuration_EnableLakeraGuard =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableLakeraGuard";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableEnkryptGuardrails setting.
        /// <para>Value description:<br/>Indicates whether Enkrypt Guardrails is available for use by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Configuration_EnableEnkryptGuardrails =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableEnkryptGuardrails";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableAzureContentSafety setting.
        /// <para>Value description:<br/>Indicates whether Azure Content Safety is available for use by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Configuration_EnableAzureContentSafety =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableAzureContentSafety";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShields setting.
        /// <para>Value description:<br/>Indicates whether Azure Content Safety Prompt Shields is available for use by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Configuration_EnableAzureContentSafetyPromptShields =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableAzureContentSafetyPromptShields";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableMicrosoftPresidio setting.
        /// <para>Value description:<br/>Indicates whether Microsoft Presidio is available for use by the Gatekeeper API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperAPI_Configuration_EnableMicrosoftPresidio =
            "FoundationaLLM:APIEndpoints:GatekeeperAPI:Configuration:EnableMicrosoftPresidio";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Gatekeeper Integration API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperIntegrationAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Gatekeeper Integration API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatekeeperIntegrationAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:GatekeeperIntegrationAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Orchestration API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Orchestration API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:OrchestrationAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AccountName setting.
        /// <para>Value description:<br/>The Azure Storage account for persisted completion requests.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_OrchestrationAPI_Configuration_CompletionRequestsStorage_AccountName =
            "FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Storage account. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_OrchestrationAPI_Configuration_CompletionRequestsStorage_AuthenticationType =
            "FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:ContainerName setting.
        /// <para>Value description:<br/>The name of the root container in the Azure Storage account.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_OrchestrationAPI_Configuration_CompletionRequestsStorage_ContainerName =
            "FoundationaLLM:APIEndpoints:OrchestrationAPI:Configuration:CompletionRequestsStorage:ContainerName";

        #endregion

        #region FoundationaLLM:APIEndpoints:LangChainAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the LangChain API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the LangChain API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:LangChainAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:PollingIntervalSeconds setting.
        /// <para>Value description:<br/>The interval in seconds at which the LangChain API will be polled for status.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Configuration_PollingIntervalSeconds =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:PollingIntervalSeconds";

        #endregion

        #region FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AccountName setting.
        /// <para>Value description:<br/>The Azure Storage account for external modules.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Configuration_ExternalModules_Storage_AccountName =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Storage account. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Configuration_ExternalModules_Storage_AuthenticationType =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:RootStorageContainer setting.
        /// <para>Value description:<br/>The name of the root container in the Azure Storage account.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Configuration_ExternalModules_RootStorageContainer =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:RootStorageContainer";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Modules setting.
        /// <para>Value description:<br/>Modules configured for external use in the LangChainAPI. The content should have the following pattern: module_name|module_tool_manager,module_name|module_tool_manager...</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_LangChainAPI_Configuration_ExternalModules_Modules =
            "FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules:Modules";

        #endregion

        #region FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Semantic Kernel API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_SemanticKernelAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Semantic Kernel API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_SemanticKernelAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:SemanticKernelAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:SemanticKernelAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:SemanticKernelAPI:Configuration:MaxConcurrentCompletions setting.
        /// <para>Value description:<br/>The maximum number of background completion operations allowed to run in parallel. If a new completion request comes in and the maximum number is already reached, the request will generate an error.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_SemanticKernelAPI_Configuration_MaxConcurrentCompletions =
            "FoundationaLLM:APIEndpoints:SemanticKernelAPI:Configuration:MaxConcurrentCompletions";

        #endregion

        #region FoundationaLLM:APIEndpoints:ManagementAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:APIUrl setting.
        /// <para>Value description:<br/>The URL of the Management API. Since it's an entry point API, the Management API does not have a corresponding APIEndpointConfiguration resource and thus the URL must be kept as a configuration setting.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Essentials_APIUrl =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:APIUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Management API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:Instance setting.
        /// <para>Value description:<br/>The URL of the Entra ID instance used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra_Instance =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:Instance";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:TenantId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID tenant used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra_TenantId =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:TenantId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:Scopes setting.
        /// <para>Value description:<br/>The list of scopes exposed by the Management API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra_Scopes =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:Scopes";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:CallbackPath setting.
        /// <para>Value description:<br/>The path where the Entra ID instance will redirect during authentication.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra_CallbackPath =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:CallbackPath";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:ClientId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID app registration used by the Management API to authenticate.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_Entra_ClientId =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:Entra:ClientId";

        #endregion

        #region FoundationaLLM:APIEndpoints:ManagementAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:AllowedUploadFileExtensions setting.
        /// <para>Value description:<br/>The comma-separated list file extensions that users are allowed to upload to a conversation.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ManagementAPI_Configuration_AllowedUploadFileExtensions =
            "FoundationaLLM:APIEndpoints:ManagementAPI:Configuration:AllowedUploadFileExtensions";

        #endregion

        #region FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Vectorization API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_VectorizationAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Vectorization API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_VectorizationAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:VectorizationAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Vectorization Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_VectorizationWorker_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Vectorization worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_VectorizationWorker_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:VectorizationWorker:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:DataPipelineAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Data Pipeline API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:DataPipelineAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Data Pipeline API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:DataPipelineAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Data Pipeline Frontend Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineFrontendWorker_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Data Pipeline Frontend Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineFrontendWorker_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Data Pipeline Backend Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineBackendWorker_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Data Pipeline Backend Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_DataPipelineBackendWorker_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:ContextAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Context API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:ContextAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Context API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:ContextAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:ContextAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:Storage:AccountName setting.
        /// <para>Value description:<br/>The Azure Storage account for the FoundationaLLM Context API file service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_Storage_AccountName =
            "FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:Storage:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Storage account. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_Storage_AuthenticationType =
            "FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Endpoint setting.
        /// <para>Value description:<br/>The URL of the Azure Cosmos DB service used by the Context API file service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_CosmosDB_Endpoint =
            "FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Endpoint";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Database setting.
        /// <para>Value description:<br/>The name of the Azure Cosmos DB database used by the Context API file service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_CosmosDB_Database =
            "FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Database";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Containers setting.
        /// <para>Value description:<br/>The comma-separated list of database containers used by the Context API file service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_CosmosDB_Containers =
            "FoundationaLLM:APIEndpoints:ContextAPI:Configuration:FileService:CosmosDB:Containers";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatewayAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Gateway API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Gateway API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:GatewayAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatewayAPI:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAccounts setting.
        /// <para>Value description:<br/>The comma-separated list of Azure OpenAI account names used by the Gateway API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAPI_Configuration_AzureOpenAIAccounts =
            "FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAccounts";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAssistantsMaxVectorizationTimeSeconds setting.
        /// <para>Value description:<br/>The maximum time in seconds allowed for an Azure OpenAI Assistants vectorization process to complete.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAPI_Configuration_AzureOpenAIAssistantsMaxVectorizationTimeSeconds =
            "FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureOpenAIAssistantsMaxVectorizationTimeSeconds";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureAIAgentServiceMaxVectorizationTimeSeconds setting.
        /// <para>Value description:<br/>The maximum time in seconds allowed for an Azure AI Agent Service vectorization process to complete.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAPI_Configuration_AzureAIAgentServiceMaxVectorizationTimeSeconds =
            "FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:AzureAIAgentServiceMaxVectorizationTimeSeconds";

        #endregion

        #region FoundationaLLM:APIEndpoints:GatewayAdapterAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAdapterAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Gateway Adapter API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAdapterAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:GatewayAdapterAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:GatewayAdapterAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the Gateway Adapter API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_GatewayAdapterAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:GatewayAdapterAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:StateAPI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl setting.
        /// <para>Value description:<br/>The URL of the State API. This entry supports the dependency of the Orchestration implementations to retrieve the URL. Python APIs do not have a resource provider implementation to retrieve the details.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Essentials_APIUrl =
            "FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the State API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:StateAPI:Essentials:APIKey";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Essentials:AppInsightsConnectionString setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the connection string for the App Insights service used by the State API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Essentials_AppInsightsConnectionString =
            "FoundationaLLM:APIEndpoints:StateAPI:Essentials:AppInsightsConnectionString";

        #endregion

        #region FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Endpoint setting.
        /// <para>Value description:<br/>The URL of the Azure Cosmos DB service used by the State API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB_Endpoint =
            "FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Endpoint";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Database setting.
        /// <para>Value description:<br/>The name of the Azure Cosmos DB database used by the State API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB_Database =
            "FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Database";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Containers setting.
        /// <para>Value description:<br/>The comma-separated list of database containers used by the State API.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB_Containers =
            "FoundationaLLM:APIEndpoints:StateAPI:Configuration:CosmosDB:Containers";

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureOpenAI:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureOpenAI:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Azure OpenAI service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureOpenAI_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:AzureOpenAI:Essentials:APIKey";

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureCosmosDBNoSQLVectorStore:Configuration

        #endregion

        #region FoundationaLLM:APIEndpoints:AzurePostgreSQLVectorStore:Configuration

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureEventGrid:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureEventGrid:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Azure Event Grid service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:AzureEventGrid:Essentials:APIKey";

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureEventGrid:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureEventGrid:Configuration:NamespaceId setting.
        /// <para>Value description:<br/>The object identifier of the Azure Event Grid Namespace object used by the Azure Event Grid event service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration_NamespaceId =
            "FoundationaLLM:APIEndpoints:AzureEventGrid:Configuration:NamespaceId";

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureAIStudio:Configuration

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureAIStudio:Configuration:Storage

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureContentSafety:Essentials
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureContentSafety:Essentials:APIKey setting.
        /// <para>Value description:<br/>The name of the Azure Key Vault secret holding the API key for the Azure Content Safety service.</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety_Essentials_APIKey =
            "FoundationaLLM:APIEndpoints:AzureContentSafety:Essentials:APIKey";

        #endregion

        #region FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:HateSeverity setting.
        /// <para>Value description:<br/>The maximum level of hate language that is allowed by the Azure Content Safety service (higher value means that a more severe language is allowed).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety_Configuration_HateSeverity =
            "FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:HateSeverity";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:SelfHarmSeverity setting.
        /// <para>Value description:<br/>The maximum level of self-harm language that is allowed by the Azure Content Safety service (higher value means that a more severe language is allowed).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety_Configuration_SelfHarmSeverity =
            "FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:SelfHarmSeverity";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:SexualSeverity setting.
        /// <para>Value description:<br/>The maximum level of sexual language that is allowed by the Azure Content Safety service (higher value means that a more severe language is allowed).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety_Configuration_SexualSeverity =
            "FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:SexualSeverity";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:ViolenceSeverity setting.
        /// <para>Value description:<br/>The maximum level of violent language that is allowed by the Azure Content Safety service (higher value means that a more severe language is allowed).</para>
        /// </summary>
        public const string FoundationaLLM_APIEndpoints_AzureContentSafety_Configuration_ViolenceSeverity =
            "FoundationaLLM:APIEndpoints:AzureContentSafety:Configuration:ViolenceSeverity";

        #endregion

        #region FoundationaLLM:APIEndpoints:LakeraGuard:Configuration

        #endregion

        #region FoundationaLLM:APIEndpoints:EnkryptGuardrails:Configuration

        #endregion

        #region FoundationaLLM:Branding
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:AccentColor setting.
        /// <para>Value description:<br/>Accent color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_AccentColor =
            "FoundationaLLM:Branding:AccentColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:AccentTextColor setting.
        /// <para>Value description:<br/>Accent text color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_AccentTextColor =
            "FoundationaLLM:Branding:AccentTextColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:BackgroundColor setting.
        /// <para>Value description:<br/>BackgroundColor.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_BackgroundColor =
            "FoundationaLLM:Branding:BackgroundColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:CompanyName setting.
        /// <para>Value description:<br/>Company name.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_CompanyName =
            "FoundationaLLM:Branding:CompanyName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:FavIconUrl setting.
        /// <para>Value description:<br/>Fav icon url.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_FavIconUrl =
            "FoundationaLLM:Branding:FavIconUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:AgentIconUrl setting.
        /// <para>Value description:<br/>The agent icon that displays next to the agent select list and agent responses. Can be an absolute URL, relative path, or base64string value.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_AgentIconUrl =
            "FoundationaLLM:Branding:AgentIconUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:KioskMode setting.
        /// <para>Value description:<br/>Kiosk mode.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_KioskMode =
            "FoundationaLLM:Branding:KioskMode";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:LogoText setting.
        /// <para>Value description:<br/>Logo text.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_LogoText =
            "FoundationaLLM:Branding:LogoText";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:LogoUrl setting.
        /// <para>Value description:<br/>Logo url.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_LogoUrl =
            "FoundationaLLM:Branding:LogoUrl";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:PageTitle setting.
        /// <para>Value description:<br/>Page title.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_PageTitle =
            "FoundationaLLM:Branding:PageTitle";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:PrimaryColor setting.
        /// <para>Value description:<br/>Primary color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryColor =
            "FoundationaLLM:Branding:PrimaryColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:PrimaryTextColor setting.
        /// <para>Value description:<br/>Primary text color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryTextColor =
            "FoundationaLLM:Branding:PrimaryTextColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:SecondaryColor setting.
        /// <para>Value description:<br/>Secondary color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryColor =
            "FoundationaLLM:Branding:SecondaryColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:SecondaryTextColor setting.
        /// <para>Value description:<br/>Secondary text color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryTextColor =
            "FoundationaLLM:Branding:SecondaryTextColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:PrimaryButtonBackgroundColor setting.
        /// <para>Value description:<br/>Primary button background color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryButtonBackgroundColor =
            "FoundationaLLM:Branding:PrimaryButtonBackgroundColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:PrimaryButtonTextColor setting.
        /// <para>Value description:<br/>Primary button text color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_PrimaryButtonTextColor =
            "FoundationaLLM:Branding:PrimaryButtonTextColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:SecondaryButtonBackgroundColor setting.
        /// <para>Value description:<br/>Secondary button background color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryButtonBackgroundColor =
            "FoundationaLLM:Branding:SecondaryButtonBackgroundColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:SecondaryButtonTextColor setting.
        /// <para>Value description:<br/>Secondary button text color.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_SecondaryButtonTextColor =
            "FoundationaLLM:Branding:SecondaryButtonTextColor";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:FooterText setting.
        /// <para>Value description:<br/>Footer text.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_FooterText =
            "FoundationaLLM:Branding:FooterText";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:NoAgentsMessage setting.
        /// <para>Value description:<br/>Message to display to users when their agent list is empty.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_NoAgentsMessage =
            "FoundationaLLM:Branding:NoAgentsMessage";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Branding:DefaultAgentWelcomeMessage setting.
        /// <para>Value description:<br/>Message to display when starting a new conversation with an agent. This is displayed when the agent is not selected or if the selected agent does not have a configured welcome message.</para>
        /// </summary>
        public const string FoundationaLLM_Branding_DefaultAgentWelcomeMessage =
            "FoundationaLLM:Branding:DefaultAgentWelcomeMessage";

        #endregion

        #region FoundationaLLM:UserPortal:Authentication:Entra
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:Instance setting.
        /// <para>Value description:<br/>The URL of the Entra ID instance used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_Instance =
            "FoundationaLLM:UserPortal:Authentication:Entra:Instance";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:TenantId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID tenant used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_TenantId =
            "FoundationaLLM:UserPortal:Authentication:Entra:TenantId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:Scopes setting.
        /// <para>Value description:<br/>The list of scopes used to get the authentication token for the Core API.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_Scopes =
            "FoundationaLLM:UserPortal:Authentication:Entra:Scopes";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:CallbackPath setting.
        /// <para>Value description:<br/>The path where the Entra ID instance will redirect during authentication.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_CallbackPath =
            "FoundationaLLM:UserPortal:Authentication:Entra:CallbackPath";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:ClientId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID app registration used by the User Portal to authenticate.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_ClientId =
            "FoundationaLLM:UserPortal:Authentication:Entra:ClientId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Authentication:Entra:TimeoutInMinutes setting.
        /// <para>Value description:<br/>The timeout in minutes for the auth token in the User Portal.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Authentication_Entra_TimeoutInMinutes =
            "FoundationaLLM:UserPortal:Authentication:Entra:TimeoutInMinutes";

        #endregion

        #region FoundationaLLM:UserPortal:Configuration
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup setting.
        /// <para>Value description:<br/>If true, the last conversation will be displayed when the user logs in. Otherwise, a new conversation placeholder appears on page load.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Configuration_ShowLastConversationOnStartup =
            "FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Configuration:ShowMessageRating setting.
        /// <para>Value description:<br/>If true, rating options on agent messages will appear.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Configuration_ShowMessageRating =
            "FoundationaLLM:UserPortal:Configuration:ShowMessageRating";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Configuration:ShowMessageTokens setting.
        /// <para>Value description:<br/>If true, the number of consumed tokens on agent and user messages will appear.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Configuration_ShowMessageTokens =
            "FoundationaLLM:UserPortal:Configuration:ShowMessageTokens";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Configuration:ShowViewPrompt setting.
        /// <para>Value description:<br/>If true, the 'View Prompt' button on agent messages will appear.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Configuration_ShowViewPrompt =
            "FoundationaLLM:UserPortal:Configuration:ShowViewPrompt";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:UserPortal:Configuration:ShowFileUpload setting.
        /// <para>Value description:<br/>Global setting to determine if file upload is allowed on chat messages.</para>
        /// </summary>
        public const string FoundationaLLM_UserPortal_Configuration_ShowFileUpload =
            "FoundationaLLM:UserPortal:Configuration:ShowFileUpload";

        #endregion

        #region FoundationaLLM:ManagementPortal:Authentication:Entra
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ManagementPortal:Authentication:Entra:Instance setting.
        /// <para>Value description:<br/>The URL of the Entra ID instance used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_ManagementPortal_Authentication_Entra_Instance =
            "FoundationaLLM:ManagementPortal:Authentication:Entra:Instance";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ManagementPortal:Authentication:Entra:TenantId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID tenant used for authentication.</para>
        /// </summary>
        public const string FoundationaLLM_ManagementPortal_Authentication_Entra_TenantId =
            "FoundationaLLM:ManagementPortal:Authentication:Entra:TenantId";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ManagementPortal:Authentication:Entra:Scopes setting.
        /// <para>Value description:<br/>The list of scopes used to get the authentication token for the Management API.</para>
        /// </summary>
        public const string FoundationaLLM_ManagementPortal_Authentication_Entra_Scopes =
            "FoundationaLLM:ManagementPortal:Authentication:Entra:Scopes";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ManagementPortal:Authentication:Entra:CallbackPath setting.
        /// <para>Value description:<br/>The path where the Entra ID instance will redirect during authentication.</para>
        /// </summary>
        public const string FoundationaLLM_ManagementPortal_Authentication_Entra_CallbackPath =
            "FoundationaLLM:ManagementPortal:Authentication:Entra:CallbackPath";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:ManagementPortal:Authentication:Entra:ClientId setting.
        /// <para>Value description:<br/>The unique identifier of the Entra ID app registration used by the Management Portal to authenticate.</para>
        /// </summary>
        public const string FoundationaLLM_ManagementPortal_Authentication_Entra_ClientId =
            "FoundationaLLM:ManagementPortal:Authentication:Entra:ClientId";

        #endregion

        #region FoundationaLLM:Vectorization
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:Worker setting.
        /// <para>Value description:<br/>The processing configuration used by the Vectorization Worker service.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_Worker =
            "FoundationaLLM:Vectorization:Worker";

        #endregion

        #region FoundationaLLM:Vectorization:Steps

        #endregion

        #region FoundationaLLM:Vectorization:Queues
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:Queues:Extract:AccountName setting.
        /// <para>Value description:<br/>The Azure Queue Storage account providing the Extract queue.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Extract_AccountName =
            "FoundationaLLM:Vectorization:Queues:Extract:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:Queues:Partition:AccountName setting.
        /// <para>Value description:<br/>The Azure Queue Storage account providing the Partition queue.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Partition_AccountName =
            "FoundationaLLM:Vectorization:Queues:Partition:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:Queues:Embed:AccountName setting.
        /// <para>Value description:<br/>The Azure Queue Storage account providing the Embed queue.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Embed_AccountName =
            "FoundationaLLM:Vectorization:Queues:Embed:AccountName";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:Queues:Index:AccountName setting.
        /// <para>Value description:<br/>The Azure Queue Storage account providing the Index queue.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_Queues_Index_AccountName =
            "FoundationaLLM:Vectorization:Queues:Index:AccountName";

        #endregion

        #region FoundationaLLM:Vectorization:StateService:Storage
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType setting.
        /// <para>Value description:<br/>The type of authentication used to connect to the Azure Blob Storage account used by the Vectorization State service. Can be one of: AzureIdentity, AccountKey, or ConnectionString.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_StateService_Storage_AuthenticationType =
            "FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Vectorization:StateService:Storage:AccountName setting.
        /// <para>Value description:<br/>The name of the Azure Blob Storage account used by the Vectorization State service.</para>
        /// </summary>
        public const string FoundationaLLM_Vectorization_StateService_Storage_AccountName =
            "FoundationaLLM:Vectorization:StateService:Storage:AccountName";

        #endregion

        #region FoundationaLLM:Vectorization:TextEmbedding:Gateway

        #endregion

        #region FoundationaLLM:DataSources

        #endregion

        #region FoundationaLLM:Events:Profiles
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:CoreAPI setting.
        /// <para>Value description:<br/>The settings used by the Core API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_CoreAPI =
            "FoundationaLLM:Events:Profiles:CoreAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:OrchestrationAPI setting.
        /// <para>Value description:<br/>The settings used by the Orchestration API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_OrchestrationAPI =
            "FoundationaLLM:Events:Profiles:OrchestrationAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:ManagementAPI setting.
        /// <para>Value description:<br/>The settings used by the Management API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_ManagementAPI =
            "FoundationaLLM:Events:Profiles:ManagementAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:VectorizationAPI setting.
        /// <para>Value description:<br/>The settings used by the Vectorization API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_VectorizationAPI =
            "FoundationaLLM:Events:Profiles:VectorizationAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:VectorizationWorker setting.
        /// <para>Value description:<br/>The settings used by the Vectorization Worker to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_VectorizationWorker =
            "FoundationaLLM:Events:Profiles:VectorizationWorker";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:GatekeeperAPI setting.
        /// <para>Value description:<br/>The settings used by the Gatekeeper API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_GatekeeperAPI =
            "FoundationaLLM:Events:Profiles:GatekeeperAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:GatewayAPI setting.
        /// <para>Value description:<br/>The settings used by the Gateway API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_GatewayAPI =
            "FoundationaLLM:Events:Profiles:GatewayAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:ContextAPI setting.
        /// <para>Value description:<br/>The settings used by the Context API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_ContextAPI =
            "FoundationaLLM:Events:Profiles:ContextAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:DataPipelineAPI setting.
        /// <para>Value description:<br/>The settings used by the Data Pipeline API to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_DataPipelineAPI =
            "FoundationaLLM:Events:Profiles:DataPipelineAPI";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:DataPipelineFrontendWorker setting.
        /// <para>Value description:<br/>The settings used by the Data Pipeline Frontend Worker to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_DataPipelineFrontendWorker =
            "FoundationaLLM:Events:Profiles:DataPipelineFrontendWorker";
        
        /// <summary>
        /// The app configuration key for the FoundationaLLM:Events:Profiles:DataPipelineBackendWorker setting.
        /// <para>Value description:<br/>The settings used by the Data Pipeline Backend Worker to process Azure Event Grid events.</para>
        /// </summary>
        public const string FoundationaLLM_Events_Profiles_DataPipelineBackendWorker =
            "FoundationaLLM:Events:Profiles:DataPipelineBackendWorker";

        #endregion

        #region FoundationaLLM:Events:Profiles:CoreAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:OrchestrationAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:ManagementAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:VectorizationAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:VectorizationWorker

        #endregion

        #region FoundationaLLM:Events:Profiles:GatekeeperAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:GatewayAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:ContextAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:DataPipelineAPI

        #endregion

        #region FoundationaLLM:Events:Profiles:DataPipelineFrontendWorker

        #endregion

        #region FoundationaLLM:Events:Profiles:DataPipelineBackendWorker

        #endregion
    }
}
