using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.Rest
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's user profile endpoints.
    /// </summary>
    internal class UserProfileRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential) : CoreRESTClientBase(httpClientFactory, credential), IUserProfileRESTClient
    {
        /// <inheritdoc/>
        public async Task<IEnumerable<UserProfile>> GetUserProfilesAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync("userprofiles");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var userProfiles = JsonSerializer.Deserialize<IEnumerable<UserProfile>>(responseContent, SerializerOptions);
                return userProfiles ?? throw new InvalidOperationException("The returned user profiles are invalid.");
            }

            throw new Exception($"Failed to retrieve user profiles. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
