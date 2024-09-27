using FoundationaLLM.Common.Constants.Authorization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Defines all authorizable actions managed by the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public static class AuthorizableActions
    {
        public static readonly ReadOnlyDictionary<string, AuthorizableAction> Actions = new(
            new Dictionary<string, AuthorizableAction>()
            {
                {
                    AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Read,
                        "Read role assignments.",
                        "Authorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Write,
                        "Create or update role assignments.",
                        "Authorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Delete,
                        "Delete role assignments.",
                        "Authorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Authorization_RoleDefinitions_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Authorization_RoleDefinitions_Read,
                        "Read role definitions.",
                        "Authorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Agent_Agents_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Agent_Agents_Read,
                        "Read agents.",
                        "Agent")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Agent_Agents_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Agent_Agents_Write,
                        "Create or update agents.",
                        "Agent")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Agent_Agents_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Agent_Agents_Delete,
                        "Delete agents.",
                        "Agent")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Read,
                        "Read assistant user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Write,
                        "Create or update assistant user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_AssistantUserContexts_Delete,
                        "Delete assistant user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Read,
                        "Read file user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Write,
                        "Create or update file user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AzureOpenAI_FileUserContexts_Delete,
                        "Delete file user contexts.",
                        "AzureOpenAI")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Read,
                        "Read app configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Write,
                        "Create or update app configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_AppConfigurations_Delete,
                        "Delete app configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Read,
                        "Read key vault secrets.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Write,
                        "Create or update key vault secrets.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_KeyVaultSecrets_Delete,
                        "Delete key vault secrets.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpointConfigurations_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpointConfigurations_Read,
                        "Read API endpoint configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpointConfigurations_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpointConfigurations_Write,
                        "Create or update API endpoint configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpoinConfigurations_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Configuration_APIEndpoinConfigurations_Delete,
                        "Delete API endpoint configurations.",
                        "Configuration")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Read,
                        "Read data sources.",
                        "DataSource")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Write,
                        "Create or update data sources.",
                        "DataSource")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Delete,
                        "Delete data sources.",
                        "DataSource")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Read,
                        "Read prompts.",
                        "Prompt")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Write,
                        "Create or update prompts.",
                        "Prompt")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Prompt_Prompts_Delete,
                        "Delete prompts.",
                        "Prompt")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Read,
                        "Read vectorization pipelines.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Write,
                        "Create or update vectorization pipelines.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationPipelines_Delete,
                        "Delete vectorization pipelines.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Read,
                        "Read vectorization requests.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Write,
                        "Create or update vectorization requests.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_VectorizationRequests_Delete,
                        "Delete vectorization requests.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Read,
                        "Read vectorization content source profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Write,
                        "Create or update vectorization content source profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_ContentSourceProfiles_Delete,
                        "Delete vectorization content source profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Read,
                        "Read vectorization text partitioning profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Write,
                        "Create or update vectorization text partitioning profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextPartitioningProfiles_Delete,
                        "Delete vectorization text partitioning profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Read,
                        "Read vectorization text embedding profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Write,
                        "Create or update vectorization text embedding profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_TextEmbeddingProfiles_Delete,
                        "Delete vectorization text embedding profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Read,
                        "Read vectorization indexing profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Write,
                        "Create or update vectorization indexing profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Vectorization_IndexingProfiles_Delete,
                        "Delete vectorization indexing profiles.",
                        "Vectorization")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Read,
                        "Read chat sessions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Write,
                        "Create or update chat sessions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_ChatSessions_Delete,
                        "Delete chat sessions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Read,
                        "Read direct completions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Write,
                        "Create or update direct completions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Completion_DirectCompletions_Delete,
                        "Delete direct completions.",
                        "Completion")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Read,
                        "Read attachments.",
                        "Attachment")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Write,
                        "Create or update attachments.",
                        "Attachment")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_Attachment_Attachments_Delete,
                        "Delete attachments.",
                        "Attachment")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Read,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Read,
                        "Read AI models",
                        "AIModel")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Write,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Write,
                        "Create or update AI models.",
                        "AIModel")
                },
                {
                    AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Delete,
                    new AuthorizableAction(
                        AuthorizableActionNames.FoundationaLLM_AIModel_AIModels_Delete,
                        "Delete AI models.",
                        "AIModel")
                },
            });

        /// <summary>
        /// Selects all actions whose names match the specified action pattern.
        /// </summary>
        /// <param name="actionPattern">The action pattern used for selection.</param>
        /// <returns>The list of matching action names.</returns>
        public static List<string> GetMatchingActions(string actionPattern)
        {
            var regexPattern = actionPattern
                .Replace(".", "\\.")
                .Replace("/", "\\/")
                .Replace("*", "[a-zA-Z\\/.]*");
            regexPattern = $"^{regexPattern}$";

            return Actions.Values
                .Select(v => v.Name)
                .Where(name => Regex.IsMatch(name, regexPattern))
                .ToList();
        }
    }
}