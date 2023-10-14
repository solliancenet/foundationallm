using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Service filter attribute for X-API-Key header validation.
    /// </summary>
    public class APIKeyAuthenticationAttribute : ServiceFilterAttribute
    {
        public APIKeyAuthenticationAttribute()
            : base(typeof(APIKeyAuthenticationFilter))
        {
        }
    }
}
