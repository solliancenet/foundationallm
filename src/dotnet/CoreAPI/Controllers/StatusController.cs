using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FoundationaLLM.Core.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(ICoreService coreService,
            ILogger<StatusController> logger)
        {
            _coreService = coreService;
            _logger = logger;
        }

        [HttpGet(Name = "GetServiceStatus")]
        public string Get()
        {
            return _coreService.Status;
        }

        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Add("Allow", new[] { "GET", "POST", "OPTIONS" });
            
            return Ok();
        }
    }
}
