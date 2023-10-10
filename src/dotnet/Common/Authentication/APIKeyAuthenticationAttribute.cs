using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Common.Authentication
{
    public class APIKeyAuthenticationAttribute : ServiceFilterAttribute
    {
        public APIKeyAuthenticationAttribute()
            : base(typeof(APIKeyAuthenticationFilter))
        {
        }
    }
}
