using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Common.Authorization
{
    public class ApiKeyAuthorizationAttribute : ServiceFilterAttribute
    {
        public ApiKeyAuthorizationAttribute()
            : base(typeof(ApiKeyAuthorizationFilter))
        {
        }
    }
}
