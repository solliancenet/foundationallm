using FoundationaLLM.AuthorizationEngine.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FoundationaLLM.AuthorizationEngine.Middleware
{
    /// <summary>
    /// Middleware that retrieves context information for the current HTTP request.
    /// This middleware should be registered in the application's Startup.Configure method.
    /// </summary>
    public class AuthorizationAPIMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationAPIMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        public AuthorizationAPIMiddleware(RequestDelegate next) =>
            _next = next;

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The current HTTP request context.</param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext context,
            IAuthorizationCore authorizationCore)
        {
            if (context.User is { Identity.IsAuthenticated: true })
            {
                var userId = context.User.FindFirstValue(ClaimConstants.Oid)
                             ?? context.User.FindFirstValue(ClaimConstants.ObjectId)
                             ?? context.User.FindFirstValue(ClaimConstants.NameIdentifierId);
                if (string.IsNullOrWhiteSpace(userId)
                    || !authorizationCore.AllowAuthorizationRequestsProcessing(
                        (context.Request.RouteValues["instanceId"] as string)!,
                        userId))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Authorization requests processing is not allowed.");

                    return;
                }
            }

            // Call the next delegate/middleware in the pipeline:
            await _next(context);
        }
    }
}
