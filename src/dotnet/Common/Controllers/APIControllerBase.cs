using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

        /// <summary>
        /// Gets the <see cref="UnifiedUserIdentity"/> of the current user. If there is an available
        /// JWT token that contains the user's identity, it will be extracted and returned. Otherwise,
        /// the user's identity will be extracted from the HTTP headers.
        /// </summary>
        protected UnifiedUserIdentity? UserIdentity
        {
            get
            {
                if (_userIdentity == null)
                {
                    if (User != null)
                    {
                        // Extract from ClaimsPrincipal if available (e.g., in CoreAPI)
                        _userIdentity = _claimsProviderService.GetUserIdentity(User);
                    }
                    else
                    {
                        // Extract from HTTP headers if available (e.g., in downstream APIs)
                        string serializedIdentity = HttpContext.Request.Headers[Constants.HttpHeaders.UserIdentity].ToString();
                        if (!string.IsNullOrEmpty(serializedIdentity))
                        {
                            _userIdentity = JsonConvert.DeserializeObject<UnifiedUserIdentity>(serializedIdentity);
                        }
                    }
                }
                return _userIdentity;
            }
        }
    }
}
