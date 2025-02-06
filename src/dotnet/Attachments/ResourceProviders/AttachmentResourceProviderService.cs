using FluentValidation;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Attachment.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Attachment resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AttachmentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<AttachmentReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AttachmentResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: false)
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AttachmentResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_Attachment;

        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null)
        {
            var policyDefinition = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resources.
            // The implementation of the PEP is straightforward: the resource provider loads the resources from the data store matching the UPN of the user identity.

            switch (resourcePath.MainResourceTypeName)
            {
                case AttachmentResourceTypeNames.Attachments:
                    var attachments = new List<AttachmentReference>();

                    if (resourcePath.ResourceTypeInstances[0].ResourceId != null)
                        attachments = [await _cosmosDBService.GetAttachment(userIdentity.UPN!, resourcePath.ResourceTypeInstances[0].ResourceId!)];
                    else
                        attachments = await _cosmosDBService.GetAttachments(userIdentity.UPN!);

                    var attachmentFiles = new List<AttachmentFile>();
                    foreach(var attachment in attachments)
                        attachmentFiles.Add(await LoadAttachment(attachment, options != null && options!.LoadContent));

                    return attachmentFiles.Select(af => new ResourceProviderGetResult<AttachmentFile>
                    {
                        Resource = af,
                        Roles = (options?.IncludeRoles ?? false)
                              ? authorizationResult.Roles
                              : [],
                        Actions = (options?.IncludeActions ?? false)
                                ? authorizationResult.Actions
                                : []
                    }).ToList();

                case AttachmentResourceTypeNames.AgentPrivateFiles:
                    var agentPrivateFiles = new List<AttachmentReference>();

                    if (resourcePath.ResourceTypeInstances[0].ResourceId != null)
                        agentPrivateFiles = [await _cosmosDBService.GetAttachment(userIdentity.UPN!, resourcePath.ResourceTypeInstances[0].ResourceId!)];
                    else
                        agentPrivateFiles = await _cosmosDBService.GetAttachments(userIdentity.UPN!);

                    var results = new List<AgentPrivateFile>();
                    foreach (var agentPrivateFile in agentPrivateFiles)
                        results.Add(await LoadAgentPrivateFile(agentPrivateFile, options != null && options!.LoadContent));

                    return results.Select(r => new ResourceProviderGetResult<AgentPrivateFile>
                    {
                        Resource = r,
                        Roles = (options?.IncludeRoles ?? false)
                              ? authorizationResult.Roles
                              : [],
                        Actions = (options?.IncludeActions ?? false)
                                ? authorizationResult.Actions
                                : []
                    }).ToList();

                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            };
        }

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity)
        {
            var policyDefinition = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to load the resources.
            // The implementation of the PEP is straightforward: the resource provider loads the resources from the data store matching the UPN of the user identity.

            switch (resourcePath.MainResourceTypeName)
            {
                case AttachmentResourceTypeNames.Attachments:
                    switch(resourcePath.Action)
                    {
                        case ResourceProviderActions.Filter:
                            var attachments = await _cosmosDBService.FilterAttachments(
                                userIdentity.UPN!,
                                JsonSerializer.Deserialize<ResourceFilter>(serializedAction)!);

                            var results = new List<AttachmentFile>();
                            foreach (var attachment in attachments)
                                results.Add(await LoadAttachment(attachment, false));

                            return results;
                        default:
                            throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest);
                    }

                case AttachmentResourceTypeNames.AgentPrivateFiles:
                    switch (resourcePath.Action)
                    {
                        case ResourceProviderActions.Filter:
                            var agentPrivateFiles = await _cosmosDBService.FilterAttachments(
                                userIdentity.UPN!,
                                JsonSerializer.Deserialize<ResourceFilter>(serializedAction)!);

                            var results = new List<AgentPrivateFile>();
                            foreach (var agentPrivateFile in agentPrivateFiles)
                                results.Add(await LoadAgentPrivateFile(agentPrivateFile, false));

                            return results;

                        default:
                            throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                                StatusCodes.Status400BadRequest);
                    }
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            };
        }

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AttachmentResourceTypeNames.Attachments:
                case AttachmentResourceTypeNames.AgentPrivateFiles:
                    var attachment = await _cosmosDBService.GetAttachment(userIdentity.UPN!, resourcePath.ResourceTypeInstances[0].ResourceId!)
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

                    await _cosmosDBService.DeleteAttachment(attachment);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AttachmentResourceTypeNames.AgentPrivateFiles => await UpdateAgentPrivateFile(resourcePath, formFile!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null) where T : class
        {
            var attachment = await _cosmosDBService.GetAttachment(userIdentity.UPN!, resourcePath.ResourceTypeInstances[0].ResourceId!)
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

            return (await LoadAttachment(attachment, loadContent: options?.LoadContent ?? false)) as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} could not be loaded.");
        }

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            resource switch
            {
                AttachmentFile attachment => (TResult) await UpdateAttachment(resourcePath, attachment, userIdentity),
                _ => throw new ResourceProviderException(
                    $"The type {nameof(T)} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        protected override async Task<TResult> UpdateResourcePropertiesAsyncInternal<T, TResult>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, Dictionary<string, object?> propertyValues, UnifiedUserIdentity userIdentity)
        {
            _ = EnsureAndValidatePolicyDefinitions(resourcePath, authorizationResult);

            // This is the PEP (Policy Enforcement Point) where the resource provider enforces the policy definition to update the resource properties.
            // The implementation relies on using a filter predicate to ensure that the user identity is authorized to update the resource properties.

            if (typeof(T) == typeof(AttachmentFile))
            {
                var result = await _cosmosDBService.PatchItemPropertiesAsync<AttachmentReference>(
                        AzureCosmosDBContainers.Attachments,
                        userIdentity.UPN!,
                        resourcePath.MainResourceId!,
                        userIdentity.UPN!,
                        propertyValues,
                        default)
                    ?? throw new ResourceProviderException(
                        $"The {_name} resource provider did not find the {resourcePath.RawResourcePath} resource. "
                        + "This indicates that either the resource does not exist or existing policies do not allow the user to update it.",
                        StatusCodes.Status404NotFound);
                return (new ResourceProviderUpsertResult<T>
                {
                    ObjectId = resourcePath.RawResourcePath,
                    ResourceExists = true,
                    Resource = await LoadAttachment(result, false) as T
                } as TResult)!;
            }

            throw new ResourceProviderException(
                $"The upsert properties operation is not supported by the {_name} resource provider for type {typeof(T).Name}.",
                StatusCodes.Status400BadRequest);
        }

        #endregion

        #region Resource management

        private async Task<AttachmentFile> LoadAttachment(AttachmentReference attachment, bool loadContent = false)
        {
            var attachmentFile = new AttachmentFile
            {
                ObjectId = attachment.ObjectId,
                Name = attachment.Name,
                OriginalFileName = attachment.OriginalFilename,
                Type = attachment.Type,
                Path = $"{_storageContainerName}{attachment.Filename}",
                ContentType = attachment.ContentType,
                SecondaryProvider = attachment.SecondaryProvider,
                SecondaryProviderObjectId = attachment.SecondaryProviderObjectId
            };

            if (loadContent)
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    attachment.Filename,
                    default);
                attachmentFile.Content = fileContent.ToArray();
            }

            return attachmentFile;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAttachment(ResourcePath resourcePath, AttachmentFile attachmentFile, UnifiedUserIdentity userIdentity)
        {
            if (resourcePath.ResourceTypeInstances[0].ResourceId != attachmentFile.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var extension = GetFileExtension(attachmentFile.DisplayName!);
            var fullName = $"{attachmentFile.Name}{extension}";

            attachmentFile.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
            var attachment = new AttachmentReference
            {
                Id = attachmentFile.Name,
                ObjectId = attachmentFile.ObjectId,
                OriginalFilename = attachmentFile.DisplayName!,
                ContentType = attachmentFile.ContentType!,
                Name = attachmentFile.Name,
                Type = AttachmentTypes.File,
                Filename = $"/{_name}/{fullName}",
                Size = attachmentFile.Content!.Length,
                SecondaryProvider = attachmentFile.SecondaryProvider,
                UPN = userIdentity.UPN ?? string.Empty,
                Deleted = false
            };

            var validator = _resourceValidatorFactory.GetValidator(typeof(AttachmentFile));
            if (validator is IValidator attachmentValidator)
            {
                var context = new ValidationContext<object>(attachmentFile);
                var validationResult = await attachmentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            await CreateResource(
                attachment,
                new MemoryStream(attachmentFile.Content!),
                attachmentFile.ContentType ?? default);

            await _cosmosDBService.CreateAttachment(attachment);

            return new ResourceProviderUpsertResult<AttachmentFile>
            {
                ObjectId = attachmentFile!.ObjectId,
                ResourceExists = false,
                Resource = attachmentFile
            };
        }

        private static string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        private async Task<AgentPrivateFile> LoadAgentPrivateFile(AttachmentReference attachment, bool loadContent = false)
        {
            var agentPrivateFile = new AgentPrivateFile
            {
                ObjectId = attachment.ObjectId,
                Name = attachment.Name,
                Type = attachment.Type,
                ContentType = attachment.ContentType
            };

            if (loadContent)
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    attachment.Filename,
                    default);
                agentPrivateFile.Content = fileContent.ToArray();
            }

            return agentPrivateFile;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentPrivateFile(ResourcePath resourcePath, ResourceProviderFormFile formFile, UnifiedUserIdentity userIdentity)
        {
            if (formFile.BinaryContent.Length == 0)
                throw new ResourceProviderException("The attached file is not valid.",
                    StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceId != formFile.FileName)
                throw new ResourceProviderException("The resource path does not match the file name (name mismatch).",
                    StatusCodes.Status400BadRequest);

            string agentName = "TODO";
            var filePath = $"/{_name}/{_instanceSettings.Id}/{agentName}/private-file-store/{resourcePath.ResourceId!}";
            var resourceName = $"{agentName}|{resourcePath.ResourceId}";

            var agentPrivateFile = new AttachmentReference
            {
                Id = resourceName,
                ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name),
                OriginalFilename = formFile.FileName,
                ContentType = formFile.ContentType!,
                Name = resourceName,
                Type = AttachmentTypes.AgentPrivateFile,
                Filename = filePath,
                Size = formFile.BinaryContent.Length,
                UPN = userIdentity.UPN ?? string.Empty,
                Deleted = false
            };

            await CreateResource(
                agentPrivateFile,
                new MemoryStream(formFile.BinaryContent.ToArray()),
                agentPrivateFile.ContentType ?? default);

            await _cosmosDBService.CreateAttachment(agentPrivateFile);

            return new ResourceProviderUpsertResult<AgentPrivateFile>
            {
                ObjectId = agentPrivateFile!.ObjectId,
                ResourceExists = false
            };
        }

        #endregion
    }
}
