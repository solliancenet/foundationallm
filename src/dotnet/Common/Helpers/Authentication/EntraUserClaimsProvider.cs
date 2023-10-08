using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Helpers.Authentication
{
    /// <summary>
    /// Provides a common interface for retrieving and resolving user claims
    /// from Microsoft Entra to a <see cref="UnifiedUserIdentity"/> object.
    /// </summary>
    public class EntraUserClaimsProvider : IUserClaimsProvider
    {
        public UnifiedUserIdentity GetUserIdentity(ClaimsPrincipal userPrincipal)
        {
            return new UnifiedUserIdentity
            {
                Name = userPrincipal.FindFirstValue("name"),
                Username = userPrincipal.FindFirstValue("preferred_username")
            };
        }
    }
}
