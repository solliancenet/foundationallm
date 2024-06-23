namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    public enum AuthenticationType
    {
        /// <summary>
        /// Caller is a user with account in Entra or a managed identity for service to service calls
        /// </summary>
        EntraId,
        /// <summary>
        /// Caller has a JWT access token from either client_credentials or authorization code ("code") with PKCE aurhentication with a trusted OIDC IdP
        /// </summary>
        OpenIdConnect,
        /// <summary>
        /// ApiKey shared secret
        /// </summary>
        ApiKey
    }
}
