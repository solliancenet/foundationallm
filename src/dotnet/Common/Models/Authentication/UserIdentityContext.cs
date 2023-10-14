using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// Stores a <see cref="UnifiedUserIdentity"/> object resolved from
    /// one or more services.
    /// </summary>
    public class UserIdentityContext : IUserIdentityContext
    {
        /// <summary>
        /// The current <see cref="UnifiedUserIdentity"/> object resolved
        /// from one or more services.
        /// </summary>
        public UnifiedUserIdentity? CurrentUserIdentity { get; set; }
    }
}
