using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using FoundationaLLM.Common.Models.Metadata;
using Microsoft.AspNetCore.Authentication;

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
        /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
        /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, IUserClaimsProviderService claimsProviderService, ICallContext callContext)
        {
            if (context.User is { Identity.IsAuthenticated: true })
            {
                // Extract from ClaimsPrincipal if available:
                callContext.CurrentUserIdentity = claimsProviderService.GetUserIdentity(context.User);
                callContext.Token = await context.GetTokenAsync("access_token");
            }
            else
            {
                // Extract from HTTP headers if available:
                var serializedIdentity = context.Request.Headers[Constants.HttpHeaders.UserIdentity].ToString();
                if (!string.IsNullOrEmpty(serializedIdentity))
                {
                    callContext.CurrentUserIdentity = JsonConvert.DeserializeObject<UnifiedUserIdentity>(serializedIdentity)!;
                }
            }

            var agentHint = context.Request.Headers[Constants.HttpHeaders.AgentHint].FirstOrDefault();
            if (!string.IsNullOrEmpty(agentHint))
            {
                callContext.AgentHint = JsonConvert.DeserializeObject<Agent>(agentHint);
            }

            // Call the next delegate/middleware in the pipeline:
            await _next(context);
        }
    }

}
