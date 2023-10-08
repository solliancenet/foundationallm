using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Controllers
{
    /// <summary>
    /// Provides base functionality for all API controllers.
    /// </summary>
    public class APIControllerBase : ControllerBase
    {
        protected UnifiedUserIdentity? _userIdentity;
        protected readonly IUserClaimsProvider _claimsProvider;

        protected APIControllerBase(IUserClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        protected UnifiedUserIdentity? UserIdentity
        {
            get
            {
                if (_userIdentity == null && User != null)
                {
                    _userIdentity = _claimsProvider.GetUserIdentity(User);
                }
                return _userIdentity;
            }
        }
    }
}
