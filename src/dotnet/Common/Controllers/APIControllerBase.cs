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
        protected readonly IUserClaimsProviderService _claimsProviderService;

        protected APIControllerBase(IUserClaimsProviderService claimsProviderService)
        {
            _claimsProviderService = claimsProviderService;
        }

        protected UnifiedUserIdentity? UserIdentity
        {
            get
            {
                if (_userIdentity == null && User != null)
                {
                    _userIdentity = _claimsProviderService.GetUserIdentity(User);
                }
                return _userIdentity;
            }
        }
    }
}
