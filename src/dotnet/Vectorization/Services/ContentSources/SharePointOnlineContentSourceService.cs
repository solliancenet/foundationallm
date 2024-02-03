using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth;
using PnP.Core.Services.Builder.Configuration;
using PnP.Core.Services;
using System.Security.Cryptography.X509Certificates;
using System;
using PnP.Core.Model.SharePoint;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Implements a vectorization content source for content residing in SharePoint Online.
    /// </summary>
    public class SharePointOnlineContentSourceService : ContentSourceServiceBase, IContentSourceService
    {
        private readonly SharePointOnlineContentSourceServiceSettings _settings;
        private readonly ILogger<SharePointOnlineContentSourceService> _logger;
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// Creates a new instance of the vectorization content source.
        /// </summary>
        public SharePointOnlineContentSourceService(
            SharePointOnlineContentSourceServiceSettings settings,
            ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _logger = loggerFactory.CreateLogger<SharePointOnlineContentSourceService>();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// contentId[0] = the URL of the SharePoint online tenant.
        /// contentId[1] = the relative path of the site/subsite.
        /// contentId[2] = the folder path, starting with the document library.
        /// contentId[3] = the name of the file.
        /// </remarks>
        public async Task<string> ExtractTextFromFileAsync(ContentIdentifier contentId, CancellationToken cancellationToken)
        {
            contentId.ValidateMultipartId(4);
            await EnsureServiceProvider($"{contentId[0]}/{contentId[1]}");

            var binaryContent = await GetDocumentBinaryContent(
                $"{contentId[2]}/{contentId[3]}",
                cancellationToken);

            return await ExtractTextFromFileAsync(contentId.FileName, binaryContent);
        }

        /// <summary>
        /// Retrieves the binary content of the specified SharePoint Online document.
        /// </summary>

        /// <param name="documentRelativeUrl">The server relative url of the document.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns>An object representing the binary contents of the retrieved document.</returns>
        private async Task<BinaryData> GetDocumentBinaryContent(string documentRelativeUrl, CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider!.CreateScope())
            {
                var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();

                using (var context = await pnpContextFactory.CreateAsync("Default"))
                {
                    string documentUrl = $"{context.Uri.PathAndQuery}/{documentRelativeUrl}".Replace("//", "/");
                    // Get a reference to the file
                    IFile testDocument = await context.Web.GetFileByServerRelativeUrlAsync(documentUrl);

                    Stream downloadedContentStream = await testDocument.GetContentAsync(true);
                    var binaryData = await BinaryData.FromStreamAsync(downloadedContentStream, cancellationToken);

                    return binaryData;
                }
            }
        }

        /// <summary>
        /// Retrieves a X.509 certificate from the specified Azure KeyVault.
        /// </summary>
        /// <returns>The X.509 certificate.</returns>
        private async Task<X509Certificate2> GetCertificate()
        {
            ValidateSettings();

            var certificateClient = new CertificateClient(new Uri(_settings.KeyVaultURL!), new DefaultAzureCredential());
            var certificateWithPolicy = await certificateClient.GetCertificateAsync(_settings.CertificateName);
            var certificateIdentifier = new KeyVaultSecretIdentifier(certificateWithPolicy.Value.SecretId);

            var secretClient = new SecretClient(new Uri(_settings.KeyVaultURL!), new DefaultAzureCredential());
            var secret = await secretClient.GetSecretAsync(certificateIdentifier.Name, certificateIdentifier.Version);
            var secretBytes = Convert.FromBase64String(secret.Value.Value);

            return new X509Certificate2(secretBytes);
        }

        private void ValidateSettings()
        {
            if (_settings == null)
                throw new VectorizationException("Missing configuration settings for the SharePointOnlineContentSourceService.");

            if (string.IsNullOrWhiteSpace(_settings.ClientId))
                throw new VectorizationException("Missing ClientId in the SharePointOnlineContentSourceService configuration settings.");

            if (string.IsNullOrWhiteSpace(_settings.TenantId))
                throw new VectorizationException("Missing TenantId in the SharePointOnlineContentSourceService configuration settings.");

            if (string.IsNullOrWhiteSpace(_settings.KeyVaultURL))
                throw new VectorizationException("Missing KeyVaultURL in the SharePointOnlineContentSourceService configuration settings.");

            if (string.IsNullOrWhiteSpace(_settings.CertificateName))
                throw new VectorizationException("Missing CertificateName in the SharePointOnlineContentSourceService configuration settings.");
        }

        private async Task EnsureServiceProvider(string siteUrl)
        {
            if (_serviceProvider == null)
            {
                var certificate = await GetCertificate();
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddPnPCore(options =>
                {
                    var authProvider = new X509CertificateAuthenticationProvider(
                        _settings.ClientId,
                        _settings.TenantId,
                        certificate);
                    options.DefaultAuthenticationProvider = authProvider;
                    options.Sites.Add("Default",
                        new PnPCoreSiteOptions
                        {
                            SiteUrl = siteUrl,
                            AuthenticationProvider = authProvider
                        });
                });
                services.AddPnPContextFactory();
                _serviceProvider = services.BuildServiceProvider();
            }
        }
    }
}
