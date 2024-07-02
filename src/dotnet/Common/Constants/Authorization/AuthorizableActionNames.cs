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
        /// Read API endpoints.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpoints_Read = "FoundationaLLM.Configuration/apiEndpoints/read";

        /// <summary>
        /// Create or update API endpoints.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpoints_Write = "FoundationaLLM.Configuration/apiEndpoints/write";

        /// <summary>
        /// Delete API endpoints.
        /// </summary>
        public const string FoundationaLLM_Configuration_APIEndpoints_Delete = "FoundationaLLM.Configuration/apiEndpoints/delete";

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

        #region Completion

        /// <summary>
        /// Read chat sessions.
        /// </summary>
        public const string FoundationaLLM_Completion_ChatSessions_Read = "FoundationaLLM.Completion/chatSessions/read";

        /// <summary>
        /// Create or update chat sessions.
        /// </summary>
        public const string FoundationaLLM_Completion_ChatSessions_Write = "FoundationaLLM.Completion/chatSessions/write";

        /// <summary>
        /// Delete chat sessions.
        /// </summary>
        public const string FoundationaLLM_Completion_ChatSessions_Delete = "FoundationaLLM.Completion/chatSessions/delete";

        /// <summary>
        /// Read direct completions.
        /// </summary>
        public const string FoundationaLLM_Completion_DirectCompletions_Read = "FoundationaLLM.Completion/directCompletions/read";

        /// <summary>
        /// Create or update direct completions.
        /// </summary>
        public const string FoundationaLLM_Completion_DirectCompletions_Write = "FoundationaLLM.Completion/directCompletions/write";

        /// <summary>
        /// Delete direct completions.
        /// </summary>
        public const string FoundationaLLM_Completion_DirectCompletions_Delete = "FoundationaLLM.Completion/directCompletions/delete";

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

        #endregion
    }
}
