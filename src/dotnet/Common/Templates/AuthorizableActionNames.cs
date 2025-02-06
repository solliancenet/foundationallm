namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides the names of the authorizable actions managed by the FoundationaLLM.Authorization provider.
    /// </summary>
    public static class AuthorizableActionNames
    {
        #region Authorization

        /// <summary>
        /// Read role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Read = "FoundationaLLM.Authorization/roleAssignments/read";

        /// <summary>
        /// Create or update role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Write = "FoundationaLLM.Authorization/roleAssignments/write";

        /// <summary>
        /// Delete role assignments.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleAssignments_Delete = "FoundationaLLM.Authorization/roleAssignments/delete";

        /// <summary>
        /// Read role definitions.
        /// </summary>
        public const string FoundationaLLM_Authorization_RoleDefinitions_Read = "FoundationaLLM.Authorization/roleDefinitions/read";

        #endregion

        #region Agent

        /// <summary>
        /// Read agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Read = "FoundationaLLM.Agent/agents/read";

        /// <summary>
        /// Create or update agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Write = "FoundationaLLM.Agent/agents/write";

        /// <summary>
        /// Delete agents.
        /// </summary>
        public const string FoundationaLLM_Agent_Agents_Delete = "FoundationaLLM.Agent/agents/delete";

        /// <summary>
        /// Read workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Read = "FoundationaLLM.Agent/workflows/read";

        /// <summary>
        /// Create or update workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Write = "FoundationaLLM.Agent/workflows/write";

        /// <summary>
        /// Delete workflows.
        /// </summary>
        public const string FoundationaLLM_Agent_Workflows_Delete = "FoundationaLLM.Agent/workflows/delete";

        /// <summary>
        /// Read tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Read = "FoundationaLLM.Agent/tools/read";

        /// <summary>
        /// Create or update tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Write = "FoundationaLLM.Agent/tools/write";

        /// <summary>
        /// Delete tools.
        /// </summary>
        public const string FoundationaLLM_Agent_Tools_Delete = "FoundationaLLM.Agent/tools/delete";

        #endregion

        #region AzureOpenAI

        /// <summary>
        /// Read Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Read = "FoundationaLLM.AzureOpenAI/conversationMappings/read";

        /// <summary>
        /// Create or update Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Write = "FoundationaLLM.AzureOpenAI/conversationMappings/write";

        /// <summary>
        /// Delete Azure OpenAI conversation mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_ConversationMappings_Delete = "FoundationaLLM.AzureOpenAI/conversationMappings/delete";

        /// <summary>
        /// Read Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Read = "FoundationaLLM.AzureOpenAI/fileMappings/read";

        /// <summary>
        /// Create or update Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Write = "FoundationaLLM.AzureOpenAI/fileMappings/write";

        /// <summary>
        /// Delete Azure OpenAI file mappings.
        /// </summary>
        public const string FoundationaLLM_AzureOpenAI_FileMappings_Delete = "FoundationaLLM.AzureOpenAI/fileMappings/delete";

        #endregion

        #region Configuration

        /// <summary>
        /// Read app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Read = "FoundationaLLM.Configuration/appConfigurations/read";

        /// <summary>
        /// Create or update app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Write = "FoundationaLLM.Configuration/appConfigurations/write";

        /// <summary>
        /// Delete app configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_AppConfigurations_Delete = "FoundationaLLM.Configuration/appConfigurations/delete";

        /// <summary>
        /// Read key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Read = "FoundationaLLM.Configuration/keyVaultSecrets/read";

        /// <summary>
        /// Create or update key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Write = "FoundationaLLM.Configuration/keyVaultSecrets/write";

        /// <summary>
        /// Delete key vault secrets.
        /// </summary>
        public const string FoundationaLLM_Configuration_KeyVaultSecrets_Delete = "FoundationaLLM.Configuration/keyVaultSecrets/delete";

        /// <summary>
        /// Read API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpointConfigurations_Read = "FoundationaLLM.Configuration/apiEndpointConfigurations/read";

        /// <summary>
        /// Create or update API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpointConfigurations_Write = "FoundationaLLM.Configuration/apiEndpointConfigurations/write";

        /// <summary>
        /// Delete API endpoint configurations.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpoinConfigurations_Delete = "FoundationaLLM.Configuration/apiEndpointConfigurations/delete";

        #endregion

        #region DataSource

        /// <summary>
        /// Read data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Read = "FoundationaLLM.DataSource/dataSources/read";

        /// <summary>
        /// Create or update data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Write = "FoundationaLLM.DataSource/dataSources/write";

        /// <summary>
        /// Delete data sources.
        /// </summary>
        public const string FoundationaLLM_DataSource_DataSources_Delete = "FoundationaLLM.DataSource/dataSources/delete";

        #endregion

        #region Prompt

        /// <summary>
        /// Read prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Read = "FoundationaLLM.Prompt/prompts/read";

        /// <summary>
        /// Create or update prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Write = "FoundationaLLM.Prompt/prompts/write";

        /// <summary>
        /// Delete prompts.
        /// </summary>
        public const string FoundationaLLM_Prompt_Prompts_Delete = "FoundationaLLM.Prompt/prompts/delete";

        #endregion

        #region Vectorization

        /// <summary>
        /// Read vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Read = "FoundationaLLM.Vectorization/vectorizationPipelines/read";

        /// <summary>
        /// Create or update vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Write = "FoundationaLLM.Vectorization/vectorizationPipelines/write";

        /// <summary>
        /// Delete vectorization pipelines.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationPipelines_Delete = "FoundationaLLM.Vectorization/vectorizationPipelines/delete";

        /// <summary>
        /// Read vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Read = "FoundationaLLM.Vectorization/vectorizationRequests/read";

        /// <summary>
        /// Create or update vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Write = "FoundationaLLM.Vectorization/vectorizationRequests/write";

        /// <summary>
        /// Delete vectorization requests.
        /// </summary>
        public const string FoundationaLLM_Vectorization_VectorizationRequests_Delete = "FoundationaLLM.Vectorization/vectorizationRequests/delete";

        /// <summary>
        /// Read vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Read = "FoundationaLLM.Vectorization/contentSourceProfiles/read";

        /// <summary>
        /// Create or update vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Write = "FoundationaLLM.Vectorization/contentSourceProfiles/write";

        /// <summary>
        /// Delete vectorization content source profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_ContentSourceProfiles_Delete = "FoundationaLLM.Vectorization/contentSourceProfiles/delete";

        /// <summary>
        /// Read vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Read = "FoundationaLLM.Vectorization/textPartitioningProfiles/read";

        /// <summary>
        /// Create or update vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Write = "FoundationaLLM.Vectorization/textPartitioningProfiles/write";

        /// <summary>
        /// Delete vectorization text partitioning profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextPartitioningProfiles_Delete = "FoundationaLLM.Vectorization/textPartitioningProfiles/delete";

        /// <summary>
        /// Read vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Read = "FoundationaLLM.Vectorization/textEmbeddingProfiles/read";

        /// <summary>
        /// Create or update vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Write = "FoundationaLLM.Vectorization/textEmbeddingProfiles/write";

        /// <summary>
        /// Delete vectorization text embedding profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_TextEmbeddingProfiles_Delete = "FoundationaLLM.Vectorization/textEmbeddingProfiles/delete";

        /// <summary>
        /// Read vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Read = "FoundationaLLM.Vectorization/indexingProfiles/read";

        /// <summary>
        /// Create or update vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Write = "FoundationaLLM.Vectorization/indexingProfiles/write";

        /// <summary>
        /// Delete vectorization indexing profiles.
        /// </summary>
        public const string FoundationaLLM_Vectorization_IndexingProfiles_Delete = "FoundationaLLM.Vectorization/indexingProfiles/delete";

        #endregion

        #region Attachment

        /// <summary>
        /// Read attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Read = "FoundationaLLM.Attachment/attachments/read";

        /// <summary>
        /// Create or update attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Write = "FoundationaLLM.Attachment/attachments/write";

        /// <summary>
        /// Delete attachments.
        /// </summary>
        public const string FoundationaLLM_Attachment_Attachments_Delete = "FoundationaLLM.Attachment/attachments/delete";

        /// <summary>
        /// Read agent private files.
        /// </summary>
        public const string FoundationaLLM_Attachment_AgentPrivateFiles_Read = "FoundationaLLM.Attachment/agentPrivateFiles/read";

        /// <summary>
        /// Create or update agent private files.
        /// </summary>
        public const string FoundationaLLM_Attachment_AgentPrivateFiles_Write = "FoundationaLLM.Attachment/agentPrivateFiles/write";

        /// <summary>
        /// Delete agent private files.
        /// </summary>
        public const string FoundationaLLM_Attachment_AgentPrivateFiles_Delete = "FoundationaLLM.Attachment/agentPrivateFiles/delete";

        #endregion

        #region AIModel

        /// <summary>
        /// Read AI models
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Read = "FoundationaLLM.AIModel/aiModels/read";

        /// <summary>
        /// Create or update AI models.
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Write = "FoundationaLLM.AIModel/aiModels/write";

        /// <summary>
        /// Delete AI models.
        /// </summary>
        public const string FoundationaLLM_AIModel_AIModels_Delete = "FoundationaLLM.AIModel/aiModels/delete";

        #endregion

        #region Conversation

        /// <summary>
        /// Read conversations
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Read = "FoundationaLLM.Conversation/conversations/read";

        /// <summary>
        /// Create or update conversations.
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Write = "FoundationaLLM.Conversation/conversations/write";

        /// <summary>
        /// Delete conversations.
        /// </summary>
        public const string FoundationaLLM_Conversation_Conversations_Delete = "FoundationaLLM.Conversation/conversations/delete";

        #endregion
    }
}
