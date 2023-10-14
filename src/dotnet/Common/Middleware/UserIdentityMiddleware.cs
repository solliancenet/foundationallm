using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Middleware
{
    /// <summary>
    /// Provides user identity information to the application by extracting
    /// the user identity from the HTTP request and resolving it to a
    /// <see cref="UnifiedUserIdentity"/> object stored in the <see cref="IUserIdentityContext"/>.
    /// This enables the application to access the user identity information in a uniform way,
    /// whether the user identity is provided in the HTTP headers or in the JWT token, and whether
    /// accessing the user identity from a Controller action or from a service.
    /// This middleware should be registered in the application's <see cref="Startup.Configure"/>.
    /// </summary>
    public class UserIdentityMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserIdentityMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        public UserIdentityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <param name="claimsProviderService">Resolves user claims to a <see cref="UnifiedUserIdentity"/> object.</param>
        /// <param name="userIdentityContext">Stores the resolved <see cref="UnifiedUserIdentity"/> object populated
        /// by this middleware.</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, IUserClaimsProviderService claimsProviderService, IUserIdentityContext userIdentityContext)
        {
            if (context.User is {Identity.IsAuthenticated: true})
            {
                // Extract from ClaimsPrincipal if available:
                userIdentityContext.CurrentUserIdentity = claimsProviderService.GetUserIdentity(context.User);
            }
            else
            {
                // Extract from HTTP headers if available:
                string serializedIdentity = context.Request.Headers[Constants.HttpHeaders.UserIdentity].ToString();
                if (!string.IsNullOrEmpty(serializedIdentity))
                {
                    userIdentityContext.CurrentUserIdentity = JsonConvert.DeserializeObject<UnifiedUserIdentity>(serializedIdentity);
                }
            }

            // Call the next delegate/middleware in the pipeline:
            await _next(context);
        }
    }

}
