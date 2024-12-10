using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Instance;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Common.Middleware
{
    /// <summary>
    /// Middleware that stores context information for the current HTTP request.
    /// This middleware should be registered in the application's Startup.Configure method.
    /// </summary>
    public class CallContextMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallContextMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        public CallContextMiddleware(RequestDelegate next) =>
            _next = next;

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <param name="claimsProviderService">Resolves user claims to a <see cref="UnifiedUserIdentity"/> object.</param>
        /// <param name="identityManagementService">Provides group membership services for user principals.</param>
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <param name="instanceSettings">Contains the FoundationaLLM instance configuration settings.</param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext context,
            IUserClaimsProviderService claimsProviderService,
            IIdentityManagementService identityManagementService,
            ICallContext callContext,
            IOptions<InstanceSettings> instanceSettings,
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            if (context.User is { Identity.IsAuthenticated: true })
            {
                // Extract from ClaimsPrincipal if available:
                var userIdentity = claimsProviderService.GetUserIdentity(context.User);

                // We are only expanding group membership for User objects
                // Service Principal permissions must be assigned directly and not over group membership.
                if (userIdentity != null
                    && !claimsProviderService.IsServicePrincipal(context.User))
                {
                    switch(instanceSettings.Value.SecurityGroupRetrievalStrategy)
                    {
                        case SecurityGroupRetrievalStrategies.IdentityManagementService:
                            userIdentity.GroupIds = await identityManagementService.GetGroupsForPrincipal(
                                userIdentity.UserId!);
                            break;
                        case SecurityGroupRetrievalStrategies.AccessToken:
                            userIdentity.GroupIds = claimsProviderService.GetSecurityGroupIds(context.User) ?? [];
                            break;
                        case SecurityGroupRetrievalStrategies.None:
                        default:
                            break;
                    }
                }

                callContext.CurrentUserIdentity = userIdentity;

                // Check if the conditions for identity substitution are met.
                if (string.Compare(
                        userIdentity!.UserId,
                        instanceSettings.Value.IdentitySubstitutionSecurityPrincipalId,
                        StringComparison.OrdinalIgnoreCase) == 0
                    && !string.IsNullOrWhiteSpace(instanceSettings.Value.IdentitySubstitutionUserPrincipalNamePattern)
                    && context.Request.Headers.TryGetValue(Constants.HttpHeaders.UserIdentity, out var serializedIdentity))
                {
                    // The user identity is allowed to substitute its identity with a value provided in the X-USER-IDENTITY header.
                    try
                    {
                        var substitutedIdentity = JsonSerializer.Deserialize<UnifiedUserIdentity>(serializedIdentity!);
                        if (substitutedIdentity != null
                            && !string.IsNullOrWhiteSpace(substitutedIdentity.UPN)
                            && Regex.IsMatch(substitutedIdentity.UPN, instanceSettings.Value.IdentitySubstitutionUserPrincipalNamePattern))
                        {
                            callContext.CurrentUserIdentity = substitutedIdentity;
                        }
                    }
                    catch
                    {
                        // Ignored.
                    }
                }
            }
            else
            {
                // Extract from HTTP headers if available:
                var serializedIdentity = context.Request.Headers[Constants.HttpHeaders.UserIdentity].ToString();
                if (!string.IsNullOrWhiteSpace(serializedIdentity))
                {
                    callContext.CurrentUserIdentity = JsonSerializer.Deserialize<UnifiedUserIdentity>(serializedIdentity)!;
                }

                if (context.Request.Headers.TryGetValue(Constants.HttpHeaders.AgentAccessToken, out var agentAccessToken))
                {
                    if (!string.IsNullOrWhiteSpace(agentAccessToken.ToString()))
                    {
                        var agentAccessTokenValue = agentAccessToken.ToString();
                        // The agent access token is used to handle requests to authenticate FoundationaLLM agents.

                        var validationResult = await ValidateAgentAccessToken(agentAccessTokenValue, callContext, instanceSettings, resourceProviderServices);

                        if (!validationResult.Valid)
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Access denied. Invalid agent access token.");
                            return; // Short-circuit the request pipeline.
                        }

                        // Assign the virtual identity to the call context:
                        callContext.CurrentUserIdentity = validationResult.VirtualIdentity;
                    }
                }
            }

            callContext.InstanceId = context.Request.RouteValues["instanceId"] as string;
            if (!string.IsNullOrWhiteSpace(callContext.InstanceId) && callContext.InstanceId != instanceSettings.Value.Id)
            {
                // Throw 403 Forbidden since the instance ID within the route does not match the instance ID in the
                // configuration settings:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access denied. Invalid instance ID.");

                return; // Short-circuit the request pipeline.
            }

            // Call the next delegate/middleware in the pipeline:
            await _next(context);
        }

        private async Task<AgentAccessTokenValidationResult> ValidateAgentAccessToken(string agentAccessToken, ICallContext callContext,
            IOptions<InstanceSettings> instanceSettings,
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            var result = new AgentAccessTokenValidationResult
            {
                Valid = false,
                VirtualIdentity = null
            };
            var providerServices = resourceProviderServices.ToList();
            if (providerServices.Any(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Agent))
            {
                var agentResourceProvider =
                    providerServices.Single(rps =>
                        rps.Name == ResourceProviderNames.FoundationaLLM_Agent);
                var request = new AgentAccessTokenValidationRequest
                {
                    AccessToken = agentAccessToken
                };
                var agentName = "";
                var parts = agentAccessToken.Split('.');

                if (parts.Length > 2)
                {
                    agentName = parts[1]; // The agent name exists between the first two periods.
                }

                if (string.IsNullOrWhiteSpace(agentName))
                {
                    return result;
                }

                var serializedResult = await agentResourceProvider.HandlePostAsync(
                    $"instances/{instanceSettings.Value.Id}/providers/{ResourceProviderNames.FoundationaLLM_Agent}/{AgentResourceTypeNames.Agents}/{agentName}/{AgentResourceTypeNames.AgentAccessTokens}/{ResourceProviderActions.Validate}",
                    JsonSerializer.Serialize(request),
                    null,
                    DefaultAuthentication.ServiceIdentity);

                return serializedResult as AgentAccessTokenValidationResult ?? result;
            }

            return result;
        }
    }

}
