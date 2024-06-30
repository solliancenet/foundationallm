using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Attachment.Models;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Attachment.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Attachment resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AttachmentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Attachment)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AttachmentResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Attachment
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AttachmentResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, AttachmentReference> _attachmentReferences = [];

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Attachment;
        private const string ATTACHMENT_REFERENCES_FILE_NAME = "_attachment-references.json";
        private const string ATTACHMENT_REFERENCES_FILE_PATH =
            $"/{ResourceProviderNames.FoundationaLLM_Attachment}/{ATTACHMENT_REFERENCES_FILE_NAME}";


        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);
            if (await _storageService.FileExistsAsync(_storageContainerName, ATTACHMENT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    ATTACHMENT_REFERENCES_FILE_PATH,
                    default);

                var resourceReferenceStore =
                    JsonSerializer.Deserialize<ResourceReferenceStore<AttachmentReference>>(
                        Encoding.UTF8.GetString(fileContent.ToArray()));

                _attachmentReferences = new ConcurrentDictionary<string, AttachmentReference>(
                        resourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    ATTACHMENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<AttachmentReference>
                    {
                        ResourceReferences = []
                    }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AttachmentResourceTypeNames.Attachments => await LoadAttachments(resourcePath.ResourceTypeInstances[0], userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<AttachmentFile>>> LoadAttachments(ResourceTypeInstance instance, UnifiedUserIdentity userIdentity)
        {
            var attachments = new List<AttachmentFile>();

            if (instance.ResourceId == null)
            {
                attachments = (await Task.WhenAll(_attachmentReferences.Values
                                         .Where(ar => !ar.Deleted)
                                         .Select(ar => LoadAttachment(ar))))
                                             .Where(a => a != null)
                                             .Select(a => a!)
                                             .ToList();

            }
            else
            {
                AttachmentFile? attachment;
                if (!_attachmentReferences.TryGetValue(instance.ResourceId, out var attachmentReference))
                {
                    attachment = await LoadAttachment(null, instance.ResourceId);
                    if (attachment != null)
                        attachments.Add(attachment);
                }
                else
                {
                    if (attachmentReference.Deleted)
                        throw new ResourceProviderException(
                            $"Could not locate the {instance.ResourceId} attachment resource.",
                            StatusCodes.Status404NotFound);

                    attachment = await LoadAttachment(attachmentReference);
                    if (attachment != null)
                        attachments.Add(attachment);
                }
            }
            return attachments.Select(attachment => new ResourceProviderGetResult<AttachmentFile>() { Resource = attachment, Actions = [], Roles = [] }).ToList();
        }

        private async Task<AttachmentFile?> LoadAttachment(AttachmentReference? attachmentReference, string? resourceId = null)
        {
            if (attachmentReference != null || !string.IsNullOrWhiteSpace(resourceId))
            {
                attachmentReference ??= new AttachmentReference
                {
                    Name = resourceId!,
                    Type = nameof(AttachmentFile),
                    Filename = $"/{_name}/{resourceId}",
                    Deleted = false
                };


                return new AttachmentFile
                {
                    Name = attachmentReference.Name,
                    Type = attachmentReference.Type,
                    Path = $"{_storageContainerName}{attachmentReference.Filename}"
                };

                if (await _storageService.FileExistsAsync(_storageContainerName, attachmentReference.Filename, default))
                {
                    var fileContent = await _storageService.ReadFileAsync(_storageContainerName, attachmentReference.Filename, default);
                    var attachment = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               typeof(AttachmentFile),
                               _serializerSettings) as AttachmentFile
                           ?? throw new ResourceProviderException($"Failed to load the attachment {attachmentReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        attachmentReference.Type = attachment.Type!;
                        _attachmentReferences.AddOrUpdate(attachmentReference.Name, attachmentReference, (k, v) => attachmentReference);
                    }

                    return attachment;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _attachmentReferences.TryRemove(attachmentReference.Name, out _);
                    return null;
                }
            }
            throw new ResourceProviderException($"Could not locate the {attachmentReference.Name} attachment resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion


        protected override async Task<object> UpsertResourceAsync<T>(ResourcePath resourcePath, T resource) 
        {
            var attachment = resource as AttachmentFile;
            if (attachment == null)
                throw new ResourceProviderException($"Invalid resource type");

            if (resourcePath.ResourceTypeInstances[0].ResourceType != AttachmentResourceTypeNames.Attachments)
            {
                throw new ResourceProviderException(
                        $"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
            return await UpdateAttachment(resourcePath, attachment);
        }



        /// <inheritdoc/>
        protected override Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) => null;

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAttachment(ResourcePath resourcePath, AttachmentFile attachment)
        {

            if (_attachmentReferences.TryGetValue(attachment.Name!, out var existingAttachmentReference)
                && existingAttachmentReference!.Deleted)
                throw new ResourceProviderException($"The attachment resource {existingAttachmentReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != attachment.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var extension = GetFileExtension(attachment.DisplayName!);
            var fullName = $"{attachment.Name}{extension}";

            var attachmentReference = new AttachmentReference
            {
                OriginalFilename = attachment.DisplayName!,
                ContentType = attachment.ContentType!,
                Name = attachment.Name,
                Type = nameof(AttachmentFile),
                Filename = $"/{_name}/{fullName}",
                Deleted = false
            };

            attachment.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(typeof(AttachmentFile));
            if (validator is IValidator attachmentValidator)
            {
                var context = new ValidationContext<object>(attachment);
                var validationResult = await attachmentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            await _storageService.WriteFileAsync(
                _storageContainerName,
                attachmentReference.Filename,
                attachment.Content,
                attachment.ContentType ?? default,
                default);

            _attachmentReferences.AddOrUpdate(attachmentReference.Name, attachmentReference, (k, v) => attachmentReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    ATTACHMENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(ResourceReferenceStore<AttachmentReference>.FromDictionary(_attachmentReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (attachment as AttachmentFile)!.ObjectId
            };
        }

        private string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case AttachmentResourceTypeNames.Attachments:
                    await DeleteAttachment(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAttachment(List<ResourceTypeInstance> instances)
        {
            if (_attachmentReferences.TryGetValue(instances.Last().ResourceId!, out var attachmentReference))
            {
                if (!attachmentReference.Deleted)
                {
                    attachmentReference.Deleted = true;

                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        attachmentReference.Filename);

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        ATTACHMENT_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(ResourceReferenceStore<AttachmentReference>.FromDictionary(_attachmentReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} attachment resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(AttachmentFile))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _attachmentReferences.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var attachmentReference);
            if (attachmentReference == null || attachmentReference.Deleted)
                throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");

            var attachment = LoadAttachment(attachmentReference).Result;
            return attachment as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }


        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Attachment:
                    foreach (var @event in e.Events)
                        await HandleAttachmentResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleAttachmentResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var attachmentReference = new AttachmentReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type =nameof(AttachmentFile),
                Deleted = false
            };

            var attachment = await LoadAttachment(attachmentReference);
            attachmentReference.Name = attachment.Name;
            attachmentReference.Type = attachment.Type!;

            _attachmentReferences.AddOrUpdate(
                attachmentReference.Name,
                attachmentReference,
                (k, v) => v);

            _logger.LogInformation("The attachment reference for the [{AttachmentName}] agent or type [{AttachmentType}] was loaded.",
                attachmentReference.Name, attachmentReference.Type);
        }

        #endregion
    }
}
