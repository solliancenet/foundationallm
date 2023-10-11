using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Contains a resolved <see cref="UnifiedUserIdentity"/> object from one
    /// or more services. 
    /// </summary>
    public interface IUserIdentityContext
    {
        /// <summary>
        /// The current <see cref="UnifiedUserIdentity"/> object resolved
        /// from one or more services.
        /// </summary>
        UnifiedUserIdentity CurrentUserIdentity { get; set; }
    }
}
