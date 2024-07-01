using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage prompt resources.
    /// </summary>
    public interface IPromptManagementClient
    {
        /// <summary>
        /// Retrieves all prompt resources.
        /// </summary>
        /// <returns>All prompt resources to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<PromptBase>>> GetPromptsAsync();

        /// <summary>
        /// Retrieves a specific prompt by name.
        /// </summary>
        /// <param name="promptName">The name of the prompt resource to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<PromptBase>> GetPromptAsync(string promptName);

        /// <summary>
        /// Checks the availability of a resource name for a prompt. If the name is available, the
        /// <see cref="ResourceNameCheckResult.Status"/> value will be "Allowed". If the name is
        /// not available, the <see cref="ResourceNameCheckResult.Status"/> value will be "Denied" and
        /// the <see cref="ResourceNameCheckResult.Message"/> will explain the reason why. Typically,
        /// a denied name is due to a name conflict with an existing prompt or a prompt that was
        /// deleted but not purged.
        /// </summary>
        /// <param name="resourceName">Contains the name of the resource to check.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the required properties within the argument
        /// are empty or missing.</exception>
        Task<ResourceNameCheckResult> CheckPromptNameAsync(ResourceName resourceName);

        /// <summary>
        /// Purges a deleted prompt by its name. This action is irreversible.
        /// </summary>
        /// <param name="promptName">The name of the prompt to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgePromptAsync(string promptName);

        /// <summary>
        /// Upserts a prompt resource. If a prompt does not exist, it will be created. If a prompt
        /// does exist, it will be updated.
        /// </summary>
        /// <param name="prompt">The prompt resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertPromptAsync(PromptBase prompt);

        /// <summary>
        /// Deletes a prompt resource by name. Please note that all deletes are soft deletes. The
        /// resource will be marked as deleted but not purged. To permanently remove a resource,
        /// execute the <see cref="PurgePromptAsync"/> method with the same name.
        /// </summary>
        /// <param name="promptName">The name of the prompt resource to delete.</param>
        /// <returns></returns>
        Task DeletePromptAsync(string promptName);
    }
}
