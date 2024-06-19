using Azure.Core;
using Azure.Identity;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Provides the default credentials for authentication.
    /// </summary>
    public static class DefaultAuthentication
    {
        /// <summary>
        /// Initializes the default authentication.
        /// </summary>
        /// <param name="production">Indicates whether the environment is production or not.</param>
        public static void Initialize(bool production, string serviceName)
        {
            Production = production;

            AzureCredential = Production
                ? new ManagedIdentityCredential(Environment.GetEnvironmentVariable(EnvironmentVariables.AzureClientId))
                : new AzureCliCredential();

            var tokenResult = AzureCredential.GetToken(
                new([$"{ScopeURIs.FoundationaLLM_Authorization}/.default"]),
                default);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(tokenResult.Token) as JwtSecurityToken;
            var id = token!.Claims.First(c => c.Type == ClaimConstants.Oid)?.Value
                ?? token.Claims.First(c => c.Type == ClaimConstants.ObjectId)?.Value
                ?? token.Claims.First(c => c.Type == ClaimConstants.NameIdentifierId)?.Value;

            ServiceIdentity = new UnifiedUserIdentity
            {
                Name = serviceName,
                UserId = id,
                GroupIds = []
            };
        }

        /// <summary>
        /// Indicates whether the environment we run in is production or not.
        /// </summary>
        public static bool Production {  get; set; }

        /// <summary>
        /// The default Azure credential to use for authentication.
        /// </summary>
        public static TokenCredential? AzureCredential { get; set; }

        /// <summary>
        /// The <see cref="UnifiedUserIdentity"/> of the service based on its managed identity."/>
        /// </summary>
        public static UnifiedUserIdentity? ServiceIdentity { get; set; }
    }
}
