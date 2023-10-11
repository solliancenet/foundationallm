using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Auth filter for X-API-Key header validation.
    /// </summary>
    public class APIKeyAuthenticationFilter : IAuthorizationFilter
    {
        private readonly IAPIKeyValidationService _apiKeyValidation;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public APIKeyAuthenticationFilter(IAPIKeyValidationService apiKeyValidation)
        {
            _apiKeyValidation = apiKeyValidation;
        }

        /// <summary>
        /// Override for default OnAuthorization step to set UnauthorizedObjectResult on the context if the required header is missing or invalid.
        /// </summary>
        /// <param name="context">The context containing the HTTP request headers.</param>
        /// <returns></returns>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string userApiKey = context.HttpContext.Request.Headers[API_KEY_HEADER_NAME].ToString();

            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                context.Result = new UnauthorizedObjectResult($"The {API_KEY_HEADER_NAME} header is missing.");
                return;
            }

            if (!_apiKeyValidation.IsValid(userApiKey))
                context.Result = new UnauthorizedObjectResult($"The provided {API_KEY_HEADER_NAME} is invalid.");
        }
    }
}
