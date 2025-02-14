using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Provides methods to manage resources.
    /// </summary>
    /// <param name="callContext">The call context containing user identity details.</param>
    /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Consumes("application/json","multipart/form-data")]
    [Produces("application/json")]
    [Route($"instances/{{instanceId}}/providers/{{resourceProvider}}")]
    public class ResourceController(
        ICallContext callContext,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger<ResourceController> logger) : Controller
    {
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
        private readonly ILogger<ResourceController> _logger = logger;
        private readonly ICallContext _callContext = callContext;

        /// <summary>
        /// Gets one or more resources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <returns></returns>
        [HttpGet("{*resourcePath}", Name = "GetResources")]
        public async Task<IActionResult> GetResources(string instanceId, string resourceProvider, string resourcePath) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    var result = await resourceProviderService.HandleGetAsync(
                        $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                        _callContext.CurrentUserIdentity!,
                        new ResourceProviderGetOptions
                        {
                            IncludeActions = true,
                            IncludeRoles = true
                        });
                    return new OkObjectResult(result);
                });

        /// <summary>
        /// Creates or updates resources.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <param name="serializedResource">The optional serialized resource to be created or updated.</param>
        /// <param name="formFile">The optional file attached to the request.</param>
        /// <returns>The ObjectId of the created or updated resource.</returns>
        [HttpPost("{*resourcePath}", Name = "UpsertResource")]
        public async Task<IActionResult> UpsertResource(string instanceId, string resourceProvider, string resourcePath) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    var formFiles = HttpContext.Request.HasFormContentType ? HttpContext.Request.Form?.Files : null;
                    IFormFile? formFile = (formFiles != null && formFiles.Count > 0) ? formFiles[0] : null;

                    Dictionary<string, string>? formPayload = null;
                    if (HttpContext.Request.HasFormContentType)
                        formPayload = HttpContext.Request.Form?.Keys.ToDictionary(k => k, v => HttpContext.Request.Form[v].ToString());

                    var bodyContent = await (new StreamReader(HttpContext.Request.Body)).ReadToEndAsync();
                    string? serializedResource = !string.IsNullOrWhiteSpace(bodyContent) ? bodyContent : null;

                    if ((formFile == null || formFile.Length == 0) && serializedResource == null)
                        throw new ResourceProviderException("The serialized resource and the attached file cannot be null at the same time.", StatusCodes.Status400BadRequest);

                    ResourceProviderFormFile? resourceProviderFormFile = default;
                    if (formFile != null && formFile.Length > 0)
                    {
                        await using var stream = formFile.OpenReadStream();
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        resourceProviderFormFile = new()
                        {
                            FileName = formFile.FileName,
                            ContentType = formFile.ContentType,
                            BinaryContent = new ReadOnlyMemory<byte>(memoryStream.ToArray()),
                            Payload = formPayload
                        };
                    }

                    var result = await resourceProviderService.HandlePostAsync(
                        $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                        serializedResource?.ToString(),
                        resourceProviderFormFile,
                        _callContext.CurrentUserIdentity!);
                    return new OkObjectResult(result);
                });

        /// <summary>
        /// Deletes a resource.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProvider">The name of the resource provider that should handle the request.</param>
        /// <param name="resourcePath">The logical path of the resource type.</param>
        /// <returns></returns>
        [HttpDelete("{*resourcePath}", Name = "DeleteResource")]
        public async Task<IActionResult> DeleteResource(string instanceId, string resourceProvider, string resourcePath) =>
            await HandleRequest(
                resourceProvider,
                resourcePath,
                async (resourceProviderService) =>
                {
                    await resourceProviderService.HandleDeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}", _callContext.CurrentUserIdentity);
                    return new OkResult();
                });

        private async Task<IActionResult> HandleRequest(string resourceProvider, string resourcePath, Func<IResourceProviderService, Task<IActionResult>> handler)
        {
            if (!_resourceProviderServices.TryGetValue(resourceProvider, out var resourceProviderService))
                return new NotFoundResult();

            try
            {
                return await handler(resourceProviderService);
            }
            catch (ResourceProviderException ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The {ResourceProviderName} encountered an error while handling the request for {ResourcePath}.", resourceProvider, resourcePath);
                return StatusCode(StatusCodes.Status500InternalServerError, $"The {resourceProvider} encountered an error while handling the request for {resourcePath}.");
            }
        }
    }
}
