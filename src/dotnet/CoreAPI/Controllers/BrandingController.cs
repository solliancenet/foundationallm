using Asp.Versioning;
using FoundationaLLM.Common.Models.Configuration.Branding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.API.Controllers
{
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class BrandingController : ControllerBase
    {
        private readonly ClientBrandingConfiguration _settings;

        public BrandingController(IOptions<ClientBrandingConfiguration> settings)
        {
            _settings = settings.Value;
        }

        [AllowAnonymous]
        [HttpGet(Name = "GetBranding")]
        public IActionResult Index()
        {
            return Ok(_settings);
        }
    }
}
