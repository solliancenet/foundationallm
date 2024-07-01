using Azure.Core;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Settings;
using Microsoft.Graph.Models;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Client.Management.Clients.Rest
{
    internal class IdentityRESTClient : ManagementRESTClientBase, IIdentityRESTClient
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        public IdentityRESTClient(IHttpClientFactory httpClientFactory, TokenCredential credential)
            : base(httpClientFactory, credential) { }

        /// <inheritdoc/>
        public async Task<IEnumerable<Group>> RetrieveGroupsAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync("groups/retrieve", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<Group>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve groups. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<Group> GetGroupAsync(string groupId)
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync($"groups/{groupId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Group>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve group. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> RetrieveUsersAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync("users/retrieve", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<User>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve users. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<User> GetUserAsync(string userId)
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync($"users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve user. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DirectoryObject>> RetrieveObjectsByIdsAsync(ObjectQueryParameters parameters)
        {
            var managementClient = await GetManagementClientAsync();
            var content = new StringContent(JsonSerializer.Serialize(parameters), Encoding.UTF8, "application/json");
            var response = await managementClient.PostAsync("objects/retrievebyids", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<DirectoryObject>>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve objects by IDs. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }
    }
}
