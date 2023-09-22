﻿using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernelAPI.Controllers
{
    //[Authorize]
    [Route("api/gatekeeper")]
    [ApiController]
    public class GatekeeperController : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService;

        public GatekeeperController(
            IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _gatekeeperService.Test();

            return new OkResult();
        }
    }
}