using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing files.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class FilesController(
        ICallContext callContext,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger<FilesController> logger) : Controller
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly ILogger<FilesController> _logger = logger;
        private readonly ICallContext _callContext = callContext;
    
        /// <summary>
        /// Uploads an attachment.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="agentName">The agent name.</param>
        /// <param name="file">The file sent with the HTTP request.</param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(string instanceId, string agentName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected.");

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();

            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var resourceProviderService))
                return new NotFoundResult();

            var result = await resourceProviderService.HandlePostAsync(
                $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Agent}/{AgentResourceTypeNames.Agents}/{agentName}/{AgentResourceTypeNames.Files}/{file.FileName}",
                JsonSerializer.Serialize(
                    new AgentFile
                    {
                        Content = content,
                        Name = file.FileName,
                        DisplayName = file.FileName,
                        ContentType = file.ContentType,
                    }),
                _callContext.CurrentUserIdentity!);

            return new OkObjectResult(result);
        }
    }
}
