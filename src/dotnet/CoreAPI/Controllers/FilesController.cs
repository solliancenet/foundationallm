using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Logging;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Utils;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System.Diagnostics;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing files.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ICallContext _callContext;
        private readonly InstanceSettings _instanceSettings;
        private readonly ICoreService _coreService;
        private readonly ILogger<FilesController> _logger;

        /// <summary>
        /// The controller for managing files.
        /// </summary>
        /// <param name="callContext">The <see cref="ICallContext"/> call context of the request being handled.</param>
        /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
        /// <param name="coreService">The <see cref="ICoreService"/> core service.</param>
        /// <param name="logger"></param>
        /// <exception cref="ResourceProviderException"></exception>
        public FilesController(
            ICallContext callContext,
            IOptions<InstanceSettings> instanceOptions,
            ICoreService coreService,
            ILogger<FilesController> logger)
        {
            _callContext = callContext;
            _instanceSettings = instanceOptions.Value;
            _coreService = coreService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an attachment.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="sessionId">The session ID from which the file is uploaded.</param>
        /// <param name="agentName">The agent name.</param>
        /// <param name="file">The file sent with the HTTP request.</param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(string instanceId, string sessionId, string agentName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected.");

            var fileName = file.FileName;
            var name = $"a-{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";
            var contentType = file.ContentType;

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();

            using (var activity = ActivitySources.CoreAPIActivitySource.StartActivity("Upload", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { { "UPN", _callContext.CurrentUserIdentity?.UPN }, { "AgentName", agentName }, { "RequestId", sessionId }, { "UserId", _callContext.CurrentUserIdentity?.UserId } }))
            {
                return new OkObjectResult(
                await _coreService.UploadAttachment(
                    instanceId,
                    sessionId,
                    new AttachmentFile
                    {
                        Name = name,
                        Content = content,
                        DisplayName = fileName,
                        ContentType = contentType,
                        OriginalFileName = fileName
                    },
                    agentName,
                    _callContext.CurrentUserIdentity!));
            }
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="fileProvider">The name of the file provider.</param>
        /// <param name="fileId">The identifier of the file.</param>
        /// <returns>The file content.</returns>
        /// <remarks>
        /// The following file providers are supported:
        /// <list type="bullet">
        /// <item>FoundationaLLM.Attachments</item>
        /// <item>FoundationaLLM.AzureOpenAI</item>
        /// </list>
        /// </remarks>
        [HttpGet("{fileProvider}/{fileId}")]
        public async Task<IActionResult> Download(string instanceId, string fileProvider, string fileId)
        {
            var attachment = await _coreService.DownloadAttachment(instanceId, fileProvider, fileId, _callContext.CurrentUserIdentity!);
            using (var activity = ActivitySources.CoreAPIActivitySource.StartActivity("Download", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { { "UPN", _callContext.CurrentUserIdentity?.UPN }, { "UserId", _callContext.CurrentUserIdentity?.UserId } }))
            {
                return attachment == null
                ? NotFound()
                : File(attachment.Content!, FileMethods.GetMimeType(attachment.Content!), attachment.OriginalFileName);
            }
        }

        /// <summary>
        /// Deletes the specified file(s).
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="resourcePaths">The list of object identifiers to be deleted.</param>
        /// <returns></returns>
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(string instanceId, [FromBody] List<string> resourcePaths) =>
            new OkObjectResult(await _coreService.DeleteAttachments(instanceId, resourcePaths, _callContext.CurrentUserIdentity!));
    }
}
