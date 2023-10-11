using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoundationaLLM.Common.Authentication
{
    public class APIKeyAuthenticationFilter : IAuthorizationFilter
    {
        private readonly IAPIKeyValidationService _apiKeyValidation;

        public APIKeyAuthenticationFilter(IAPIKeyValidationService apiKeyValidation)
        {
            _apiKeyValidation = apiKeyValidation;
        }

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
