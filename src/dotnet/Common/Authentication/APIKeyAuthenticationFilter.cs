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

        /// <summary>
        /// Initializes a new instance of the APIKeyAuthenticationFilter class.
        /// </summary>
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
            string userApiKey = context.HttpContext.Request.Headers[Constants.HttpHeaders.APIKey].ToString();

            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                context.Result = new UnauthorizedObjectResult($"The {Constants.HttpHeaders.APIKey} header is missing.");
                return;
            }

            if (!_apiKeyValidation.IsValid(userApiKey))
                context.Result = new UnauthorizedObjectResult($"The provided {Constants.HttpHeaders.APIKey} is invalid.");
        }
    }
}
