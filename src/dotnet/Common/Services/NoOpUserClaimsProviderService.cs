using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// No-op implementation of <see cref="IUserClaimsProviderService"/> in cases
    /// where the user identity is not needed.
    /// </summary>
    public class NoOpUserClaimsProviderService : IUserClaimsProviderService
    {
        /// <summary>
        /// Returns null.
        /// </summary>
        /// <param name="userPrincipal"></param>
        /// <returns></returns>
        public UnifiedUserIdentity? GetUserIdentity(ClaimsPrincipal? userPrincipal)
        {
            return null;
        }
    }
}
