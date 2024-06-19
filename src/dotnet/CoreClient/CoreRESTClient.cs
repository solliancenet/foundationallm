using Azure.Core;
using FoundationaLLM.Client.Core.Clients.Rest;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Client.Core
{
    /// <inheritdoc/>
    public class CoreRESTClient : ICoreRESTClient
    {
        private readonly string _coreUri;
        private readonly TokenCredential _credential;
        private readonly APIClientSettings _options;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public CoreRESTClient()
        {
            _coreUri = null!;
            _options = null!;
            _credential = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClients.CoreAPI"/>) based on the passed in URL.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        public CoreRESTClient(string coreUri, TokenCredential credential)
            : this(coreUri, credential, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClients.CoreAPI"/>) based on the passed in URL
        /// and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreRESTClient(string coreUri, TokenCredential credential, APIClientSettings options)
        {
            _coreUri = coreUri ?? throw new ArgumentNullException(nameof(coreUri));
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var services = new ServiceCollection();
            ConfigureHttpClient(services, coreUri, options);

            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            InitializeClients(httpClientFactory);
        }

        /// <inheritdoc/>
        public ISessionRESTClient Sessions { get; private set; } = null!;
        /// <inheritdoc/>
        public IAttachmentRESTClient Attachments { get; private set; } = null!;
        /// <inheritdoc/>
        public IBrandingRESTClient Branding { get; private set; } = null!;
        /// <inheritdoc/>
        public ICompletionRESTClient Completions { get; private set; } = null!;
        /// <inheritdoc/>
        public IStatusRESTClient Status { get; private set; } = null!;
        /// <inheritdoc/>
        public IUserProfileRESTClient UserProfiles { get; private set; } = null!;

        private static void ConfigureHttpClient(IServiceCollection services, string coreUri, APIClientSettings options) =>
            services.AddHttpClient(HttpClients.CoreAPI, client =>
            {
              client.BaseAddress = new Uri(coreUri);
              client.Timeout = options.Timeout ?? TimeSpan.FromSeconds(900);
            }).AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
            {
                CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
            });

        private void InitializeClients(IHttpClientFactory httpClientFactory)
        {
            Sessions = new SessionRESTClient(httpClientFactory, _credential);
            Attachments = new AttachmentRESTClient(httpClientFactory, _credential);
            Branding = new BrandingRESTClient(httpClientFactory, _credential);
            Completions = new CompletionRESTClient(httpClientFactory, _credential);
            Status = new StatusRESTClient(httpClientFactory, _credential);
            UserProfiles = new UserProfileRESTClient(httpClientFactory, _credential);
        }
    }
}
