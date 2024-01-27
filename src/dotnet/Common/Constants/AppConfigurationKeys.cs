using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Contains constants of the keys for all app configuration settings.
    /// </summary>
    public static class AppConfigurationKeys
    {
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
        /// The key for the FoundationaLLM:APIs:AgentFactoryAPI:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI_APIKey = "FoundationaLLM:APIs:AgentFactoryAPI:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentFactoryAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI_APIUrl = "FoundationaLLM:APIs:AgentFactoryAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection app configuration setting.
        /// By default, the Agent Factory API forces HTTPS redirection. To override this behavior and allow it to handle HTTP requests, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI_ForceHttpsRedirection = "FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection";
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
        /// The key for the FoundationaLLM:APIs:CoreAPI:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI_APIUrl = "FoundationaLLM:APIs:CoreAPI:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_CoreAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString";
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
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio app configuration setting.
        /// By default, the Gatekeeper API has Microsoft Presidio integration enabled. To disable this feature, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_Configuration_EnableMicrosoftPresidio = "FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio";
        /// <summary>
        /// The key for the FoundationaLLM:APIs:GatekeeperAPI:ForceHttpsRedirection app configuration setting.
        /// By default, the Gatekeeper API forces HTTPS redirection. To override this behavior and allow it to handle HTTP requests, set this value to false.
        /// </summary>
        public const string FoundationaLLM_APIs_GatekeeperAPI_ForceHttpsRedirection = "FoundationaLLM:APIs:GatekeeperAPI:ForceHttpsRedirection";
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
        /// The key for the FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI_AppInsightsConnectionString = "FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString";
        /// <summary>
        /// The  key for the FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker_AppInsightsConnectionString = "FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:AppConfig:ConnectionString app configuration setting.
        /// This is Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_AppConfig_ConnectionString = "FoundationaLLM:AppConfig:ConnectionString";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:APIKey app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_APIKey = "FoundationaLLM:AzureContentSafety:APIKey";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:APIUrl app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_APIUrl = "FoundationaLLM:AzureContentSafety:APIUrl";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:HateSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_HateSeverity = "FoundationaLLM:AzureContentSafety:HateSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:SelfHarmSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_SelfHarmSeverity = "FoundationaLLM:AzureContentSafety:SelfHarmSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:SexualSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_SexualSeverity = "FoundationaLLM:AzureContentSafety:SexualSeverity";
        /// <summary>
        /// The key for the FoundationaLLM:AzureContentSafety:ViolenceSeverity app configuration setting.
        /// </summary>
        public const string FoundationaLLM_AzureContentSafety_ViolenceSeverity = "FoundationaLLM:AzureContentSafety:ViolenceSeverity";
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
        /// The key for the FoundationaLLM:Branding:AllowAgentSelection app configuration setting.
        /// This value corresponds with the `FoundationaLLM-AllowAgentHint` feature flag. If the feature flag is `true`,
        /// then the User Portal UI uses these values to provide agent hints to the Agent Hub in completions-based
        /// requests. Otherwise, these values are ignored.
        /// </summary>
        public const string FoundationaLLM_Branding_AllowAgentSelection = "FoundationaLLM:Branding:AllowAgentSelection";
        /// <summary>
        /// This feature flag controls whether the User Portal UI allows users to select an agent hint.
        /// </summary>
        public const string FoundationaLLM_AllowAgentHint_FeatureFlag = "FoundationaLLM-AllowAgentHint";
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
        /// The key for the FoundationaLLM:CognitiveSearch:EndPoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearch_EndPoint = "FoundationaLLM:CognitiveSearch:EndPoint";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearch:IndexName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearch_IndexName = "FoundationaLLM:CognitiveSearch:IndexName";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearch:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearch_Key = "FoundationaLLM:CognitiveSearch:Key";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearch:MaxVectorSearchResults app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearch_MaxVectorSearchResults = "FoundationaLLM:CognitiveSearch:MaxVectorSearchResults";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:BlobStorageConnection app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_BlobStorageConnection = "FoundationaLLM:CognitiveSearchMemorySource:BlobStorageConnection";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:BlobStorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_BlobStorageContainer = "FoundationaLLM:CognitiveSearchMemorySource:BlobStorageContainer";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:ConfigFilePath app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_ConfigFilePath = "FoundationaLLM:CognitiveSearchMemorySource:ConfigFilePath";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:EndPoint app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_EndPoint = "FoundationaLLM:CognitiveSearchMemorySource:EndPoint";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:IndexName app configuration setting.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_IndexName = "FoundationaLLM:CognitiveSearchMemorySource:IndexName";
        /// <summary>
        /// The key for the FoundationaLLM:CognitiveSearchMemorySource:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource_Key = "FoundationaLLM:CognitiveSearchMemorySource:Key";
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
        /// The key for the FoundationaLLM:DurableSystemPrompt:BlobStorageConnection app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_DurableSystemPrompt_BlobStorageConnection = "FoundationaLLM:DurableSystemPrompt:BlobStorageConnection";
        /// <summary>
        /// The key for the FoundationaLLM:DurableSystemPrompt:BlobStorageContainer app configuration setting.
        /// </summary>
        public const string FoundationaLLM_DurableSystemPrompt_BlobStorageContainer = "FoundationaLLM:DurableSystemPrompt:BlobStorageContainer";
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
        /// The key for the FoundationaLLM:LangChainAPI:Key app configuration setting.
        /// This is a Key Vault reference.
        /// </summary>
        public const string FoundationaLLM_LangChainAPI_Key = "FoundationaLLM:LangChainAPI:Key";
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
        /// The key for the FoundationaLLM:Refinement app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Refinement = "FoundationaLLM:Refinement";
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
        /// The key section for the FoundationaLLM:Vectorization:VectorizationWorker app configuration setting.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationWorker = "FoundationaLLM:Vectorization:VectorizationWorker";
    }

    /// <summary>
    /// Contains constants of the keys filters for app configuration setting namespaces.
    /// </summary>
    public static class AppConfigurationKeyFilters
    {
        /// <summary>
        /// The key filter for the FoundationaLLM:Branding:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Branding = "FoundationaLLM:Branding:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs = "FoundationaLLM:APIs:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:VectorizationAPI:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationAPI = "FoundationaLLM:APIs:VectorizationAPI:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:APIs:VectorizationWorker:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_VectorizationWorker = "FoundationaLLM:APIs:VectorizationWorker:*";
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
        /// The key filter for the FoundationaLLM:AgentFactory:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AgentFactory = "FoundationaLLM:AgentFactory:*";
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
        /// The key filter for the FoundationaLLM:DurableSystemPrompt:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_DurableSystemPrompt = "FoundationaLLM:DurableSystemPrompt:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:CognitiveSearchMemorySource:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource = "FoundationaLLM:CognitiveSearchMemorySource:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:BlobStorageMemorySource:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_BlobStorageMemorySource = "FoundationaLLM:CoreAPI:BlobStorageMemorySource:*";
        /// <summary>
        /// The key filter for the FoundationaLLM:Vectorization:* app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization = "FoundationaLLM:Vectorization:*";
    }

    /// <summary>
    /// Contains constants of the keys sections for app configuration setting namespaces.
    /// </summary>
    public static class AppConfigurationKeySections
    {
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
        /// The key section for the FoundationaLLM:APIs:AgentFactoryAPI app configuration settings.
        /// </summary>
        public const string FoundationaLLM_APIs_AgentFactoryAPI = "FoundationaLLM:APIs:AgentFactoryAPI";
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
        /// The key section for the FoundationaLLM:APIs:AgentFactoryAPI app configuration settings.
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
        /// The key section for the FoundationaLLM:AgentFactory app configuration settings.
        /// </summary>
        public const string FoundationaLLM_AgentFactory = "FoundationaLLM:AgentFactory";
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
        public const string FoundationaLLM_AzureContentSafety = "FoundationaLLM:AzureContentSafety";
        /// <summary>
        /// The key section for the FoundationaLLM:DurableSystemPrompt app configuration settings.
        /// </summary>
        public const string FoundationaLLM_DurableSystemPrompt = "FoundationaLLM:DurableSystemPrompt";
        /// <summary>
        /// The key section for the FoundationaLLM:CognitiveSearchMemorySource app configuration settings.
        /// </summary>
        public const string FoundationaLLM_CognitiveSearchMemorySource = "FoundationaLLM:CognitiveSearchMemorySource";
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
        /// The key section for the FoundationaLLM:Vectorization:ResourceProviderService:Storage app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ResourceProviderService_Storage = "FoundationaLLM:Vectorization:ResourceProviderService:Storage";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:ContentSources app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSources = "FoundationaLLM:Vectorization:ContentSources";
        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService";

        /// <summary>
        /// The key section for the FoundationaLLM:Vectorization:AzureAISearchIndexingService app configuration settings.
        /// </summary>
        public const string FoundationaLLM_Vectorization_AzureAISearchIndexingService = "FoundationaLLM:Vectorization:AzureAISearchIndexingService";
    }
}
