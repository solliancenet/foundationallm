using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class PromptManagementClient(IManagementRESTClient managementRestClient) : IPromptManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<PromptBase>>> GetPromptsAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                PromptResourceTypeNames.Prompts
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<PromptBase>> GetPromptAsync(string promptName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"Prompt '{promptName}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<ResourceNameCheckResult> CheckPromptNameAsync(ResourceName resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
            {
                throw new ArgumentException("Resource name and type must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{PromptResourceProviderActions.CheckName}",
                resourceName
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgePromptAsync(string promptName)
        {
            if (string.IsNullOrWhiteSpace(promptName))
            {
                throw new ArgumentException("The Prompt name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}/{PromptResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertPromptAsync(PromptBase prompt) => await managementRestClient.Resources.UpsertResourceAsync(
            ResourceProviderNames.FoundationaLLM_Prompt,
            $"{PromptResourceTypeNames.Prompts}/{prompt.Name}",
                prompt
            );

        /// <inheritdoc/>
        public async Task DeletePromptAsync(string promptName) => await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}"
            );
    }
}
