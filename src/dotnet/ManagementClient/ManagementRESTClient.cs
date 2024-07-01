using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using FoundationaLLM.Client.Management.Clients.Rest;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Client.Management
{
    /// <inheritdoc/>
    public class ManagementRESTClient : IManagementRESTClient
    {
        private readonly string _managementUri;
        private readonly string _instanceId;
        private readonly TokenCredential _credential;
        private readonly APIClientSettings _options;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public ManagementRESTClient()
        {
            _managementUri = null!;
            _instanceId = null!;
            _options = null!;
            _credential = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClients.CoreAPI"/>) based on the passed in URL.
        /// </summary>
        /// <param name="managementUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        public ManagementRESTClient(
            string managementUri,
            TokenCredential credential,
            string instanceId)
            : this(managementUri, credential, instanceId,new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClients.CoreAPI"/>) based on the passed in URL
        /// and optional client settings.
        /// </summary>
        /// <param name="managementUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public ManagementRESTClient(
            string managementUri,
            TokenCredential credential,
            string instanceId,
            APIClientSettings options)
        {
            _managementUri = managementUri ?? throw new ArgumentNullException(nameof(managementUri));
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var services = new ServiceCollection();
            ConfigureHttpClient(services, managementUri, options);

            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            InitializeClients(httpClientFactory);
        }

        /// <inheritdoc/>
        public IIdentityRESTClient Identity { get; private set; } = null!;
        /// <inheritdoc/>
        public IResourceRESTClient Resources { get; private set; } = null!;
        /// <inheritdoc/>
        public IStatusRESTClient Status { get; private set; } = null!;

        private static void ConfigureHttpClient(IServiceCollection services, string managementUri, APIClientSettings options) =>
            services.AddHttpClient(HttpClients.ManagementAPI, client =>
            {
                client.BaseAddress = new Uri(managementUri);
                client.Timeout = options.Timeout ?? TimeSpan.FromSeconds(900);
            }).AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
            {
                CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
            });

        private void InitializeClients(IHttpClientFactory httpClientFactory)
        {
            Identity = new IdentityRESTClient(httpClientFactory, _credential);
            Resources = new ResourceRESTClient(httpClientFactory, _credential, _instanceId);
            Status = new StatusRESTClient(httpClientFactory, _credential);
        }
    }
}
