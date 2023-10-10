using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoundationaLLM.Common.Authentication
{
    public class APIKeyAuthenticationFilter : IAuthorizationFilter
    {
        private readonly IAPIKeyValidationService _apiKeyValidation;
        private const string API_KEY_HEADER_NAME = "X-API-Key";

        public APIKeyAuthenticationFilter(IAPIKeyValidationService apiKeyValidation)
        {
            _apiKeyValidation = apiKeyValidation;
        }

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
