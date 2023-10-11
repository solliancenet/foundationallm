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

        protected APIControllerBase()
        {

        }
    }
}
