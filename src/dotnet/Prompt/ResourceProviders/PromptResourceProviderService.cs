using System.Collections.Concurrent;
using System.Text;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Prompt.Models.Metadata;
using FoundationaLLM.Prompt.Models.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FoundationaLLM.Prompt.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Prompt resource provider.
    /// </summary>
    public class PromptResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Prompt_ResourceProviderService)] IStorageService storageService,
        ILogger<PromptResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            storageService,
            logger)
    {
        private ConcurrentDictionary<string, PromptReference> _promptReferences = [];

        private const string PROMPT_REFERENCES_FILE_NAME = "_prompt-references.json";
        private const string PROMPT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Prompt}/_prompt-references.json";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Prompt;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> _resourceTypes =>
            new()
            {
                {
                    PromptResourceTypeNames.Prompts,
                    new ResourceTypeDescriptor(PromptResourceTypeNames.Prompts)
                },
                {
                    PromptResourceTypeNames.PromptReferences,
                    new ResourceTypeDescriptor(PromptResourceTypeNames.PromptReferences)
                }
            };

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, PROMPT_REFERENCES_FILE_PATH, default);
                var promptReferenceStore = JsonConvert.DeserializeObject<PromptReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _promptReferences = new ConcurrentDictionary<string, PromptReference>(
                    promptReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonConvert.SerializeObject(new PromptReferenceStore { PromptReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        /// <inheritdoc/>
        protected override async Task<string> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances) =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.Prompts => await LoadAndSerializePrompts(instances[0]),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };

        /// <inheritdoc/>
        protected override async Task UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource)
        {
            switch (instances[0].ResourceType)
            {
                case PromptResourceTypeNames.Prompts:
                    await UpdatePrompt(instances, serializedResource);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.");
            }
        }

        private async Task<string> LoadAndSerializePrompts(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                var serializedPrompts = new List<string>();

                foreach (var promptReference in _promptReferences.Values)
                {
                    var prompt = await LoadPrompt(promptReference);
                    serializedPrompts.Add(
                        JsonConvert.SerializeObject(prompt, _serializerSettings));
                }

                return $"[{string.Join(",", [.. serializedPrompts])}]";
            }
            else
            {
                if (!_promptReferences.TryGetValue(instance.ResourceId, out var promptReference))
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} prompt resource.");

                var prompt = await LoadPrompt(promptReference);
                return JsonConvert.SerializeObject(prompt, _serializerSettings);
            }
        }

        private async Task<Models.Metadata.Prompt> LoadPrompt(PromptReference promptReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, promptReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, promptReference.Filename, default);
                return JsonConvert.DeserializeObject<Models.Metadata.Prompt>(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    _serializerSettings) as Models.Metadata.Prompt
                    ?? throw new ResourceProviderException($"Failed to load the prompt {promptReference.Name}.");
            }

            throw new ResourceProviderException($"Could not locate the {promptReference.Name} prompt resource.");
        }

        private async Task UpdatePrompt(List<ResourceTypeInstance> instances, string serializedPrompt)
        {
            var prompt = JsonConvert.DeserializeObject<Models.Metadata.Prompt>(serializedPrompt)
                ?? throw new ResourceProviderException("The object definition is invalid.");

            if (instances[0].ResourceId != prompt.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).");

            var promptReference = new PromptReference
            {
                Name = prompt.Name!,
                Filename = $"/{_name}/{prompt.Name}.json"
            };

            var deserializeObject = JsonConvert.DeserializeObject<Models.Metadata.Prompt>(serializedPrompt, _serializerSettings);
            deserializeObject!.ObjectId = GetObjectId(instances);

            await _storageService.WriteFileAsync(
                _storageContainerName,
                promptReference.Filename,
                JsonConvert.SerializeObject(deserializeObject, _serializerSettings),
                default,
                default);

            _promptReferences[promptReference.Name] = promptReference;

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    PROMPT_REFERENCES_FILE_PATH,
                    JsonConvert.SerializeObject(PromptReferenceStore.FromDictionary(_promptReferences.ToDictionary())),
                    default,
                    default);
        }

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(List<ResourceTypeInstance> instances) where T: class =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.PromptReferences => await GetPromptAsync<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };

        /// <inheritdoc/>
        protected override async Task<IList<T>> GetResourcesAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            instances[0].ResourceType switch
            {
                PromptResourceTypeNames.PromptReferences => await GetPromptsAsync<T>(instances),
                _ => throw new ResourceProviderException($"The resource type {instances[0].ResourceType} is not supported by the {_name} resource manager.")
            };


        private async Task<List<T>> GetPromptsAsync<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (typeof(T) != typeof(PromptReference))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            var promptReferences = _promptReferences.Values.Cast<PromptReference>().ToList();
            foreach (var promptReference in promptReferences)
            {
                var prompt = await LoadPrompt(promptReference);
            }

            return promptReferences.Cast<T>().ToList();
        }

        private async Task<T> GetPromptAsync<T>(List<ResourceTypeInstance> instances) where T : class
        {
            if (instances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(PromptReference))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({instances[0].ResourceType}).");

            _promptReferences.TryGetValue(instances[0].ResourceId!, out var promptReference);
            if (promptReference != null)
            {
                return promptReference as T ?? throw new ResourceProviderException(
                    $"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
            }
            throw new ResourceProviderException(
                $"The resource {instances[0].ResourceId!} of type {instances[0].ResourceType} was not found.");
        }
    }
}
