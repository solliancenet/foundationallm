using Azure.Core;
using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Collections.Generic;

namespace FoundationaLLM.Client.Management
{
    /// <inheritdoc/>
    public class ManagementClient : IManagementClient
    {
        private readonly IManagementRESTClient _managementRestClient;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public ManagementClient() =>
            _managementRestClient = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementClient"/> class with
        /// the specified Management API URI, TokenCredential, and Instance ID.
        /// </summary>
        /// <param name="managementUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        public ManagementClient(
            string managementUri,
            TokenCredential credential,
            string instanceId)
            : this(managementUri, credential, instanceId, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI, TokenCredential, and optional client settings.
        /// </summary>
        /// <param name="managementUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public ManagementClient(
            string managementUri,
            TokenCredential credential,
            string instanceId,
            APIClientSettings options)
        {
            _managementRestClient = new ManagementRESTClient(managementUri, credential, instanceId, options);
            InitializeClients();
        }

        /// <inheritdoc/>
        public IAgentManagementClient Agents { get; private set; } = null!;
        /// <inheritdoc/>
        public IAttachmentManagementClient Attachments { get; private set; } = null!;
        /// <inheritdoc/>
        public IConfigurationManagementClient Configuration { get; private set; } = null!;
        /// <inheritdoc/>
        public IDataSourceManagementClient DataSources { get; private set; } = null!;
        /// <inheritdoc/>
        public IPromptManagementClient Prompts { get; private set; } = null!;
        /// <inheritdoc/>
        public IVectorizationManagementClient Vectorization { get; private set; } = null!;

        /// <inheritdoc/>
        public async Task<T> GetResourceByObjectId<T>(string objectId) where T : ResourceBase
        {
            var resource = await _managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<T>>>(objectId);
            return resource.FirstOrDefault()?.Resource ?? throw new InvalidOperationException($"Resource with ID {objectId} not found.");
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<T>> GetResourceWithActionsAndRolesByObjectId<T>(string objectId)
            where T : ResourceBase
        {
            var resource = await _managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<T>>>(objectId);
            return resource.FirstOrDefault() ?? throw new InvalidOperationException($"Resource with ID {objectId} not found.");
        }

        private void InitializeClients()
        {
            Agents = new AgentManagementClient(_managementRestClient);
            Attachments = new AttachmentManagementClient(_managementRestClient);
            Configuration = new ConfigurationManagementClient(_managementRestClient);
            DataSources = new DataSourceManagementClient(_managementRestClient);
            Prompts = new PromptManagementClient(_managementRestClient);
            Vectorization = new VectorizationManagementClient(_managementRestClient);
        }
    }
}
