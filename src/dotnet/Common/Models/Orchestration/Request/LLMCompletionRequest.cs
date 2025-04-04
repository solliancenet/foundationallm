﻿using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// The completion request sent by the Orchestration API to any of the downstream orchestrator APIs.
    /// </summary>
    public class LLMCompletionRequest : CompletionRequestBase
    {
        private bool _valid = false;
        private bool _hasTextEmbeddingProfile = false;
        private bool _hasIndexingProfiles = false;

        private AIModelBase? _aiModel;
        private APIEndpointConfiguration? _aiModelEndpointConfiguration;
        private Dictionary<string, string>? _otherAgentsDescriptions;
        private MultipartPrompt? _prompt;
        private TextEmbeddingProfile? _textEmbeddingProfile;
        private List<IndexingProfile>? _indexingProfiles;

        /// <summary>
        /// The agent that will process the completion request.
        /// </summary>
        [JsonPropertyName("agent")]
        public required AgentBase Agent { get; set; }

        /// <summary>
        /// Dictionary of objects (indexed by names) resulting from exploding object identifiers in the Orchestration API.
        /// <para>
        /// Can also contain objects that are not the direct result of exploding an object identifier.
        /// </para>
        /// <para>
        /// The dictionary supports the following keys:
        /// <list type="bullet">
        /// <item>
        /// /instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/{name}
        /// </item>
        /// <item>
        /// /instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/{name}
        /// </item>
        /// <item>
        /// /instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/{name}
        /// </item>
        /// <item>
        /// /instances/{instanceId}/providers/FoundationaLLM.Vectorization/indexingProfiles/{name}
        /// </item>
        /// <item>
        /// /instances/{instanceId}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/{name}
        /// </item>
        /// <item>
        /// AllAgents
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        [JsonPropertyName("objects")]
        public Dictionary<string, object> Objects { get; set; } = [];

        /// <summary>
        /// A list of <see cref="AttachmentProperties"/> objects with the properties of the attachments to include with the orchestration request.
        /// </summary>
        [JsonPropertyName("attachments")]
        public List<AttachmentProperties> Attachments { get; init; } = [];

        /// <summary>
        /// Validates the content of this LLMCompletionRequest.
        /// </summary>
        /// <exception cref="OrchestrationException"></exception>
        public void Validate()
        {
            // Avoid multiple validations.
            if (_valid) return;

            if (Agent == null)
                throw new OrchestrationException("The Agent property of the completion request cannot be null.");

            if (Agent.Workflow == null)
                throw new OrchestrationException("The Workflow property of the Agent property of the completion request cannot be null.");

            if (Objects == null)
                throw new OrchestrationException("The Objects property of the completion request cannot be null.");

            if (string.IsNullOrWhiteSpace(Agent.Workflow.MainAIModelObjectId))
                throw new OrchestrationException("Invalid agent workflow main AI model object id.");

            if (!Objects.TryGetValue(
                    Agent.Workflow.MainAIModelObjectId, out var aiModelObject))
                throw new OrchestrationException("The AI model object is missing from the request's objects.");

            var aiModel = aiModelObject is JsonElement aiModelJsonElement
                ? aiModelJsonElement.Deserialize<AIModelBase>()
                : aiModelObject as AIModelBase;

            if (aiModel == null
                || string.IsNullOrWhiteSpace(aiModel.EndpointObjectId)
                || string.IsNullOrWhiteSpace(aiModel.DeploymentName)
                || aiModel.ModelParameters == null)
                throw new OrchestrationException("The AI model object provided in the request's objects is invalid.");

            if (!Objects.TryGetValue(
                    aiModel.EndpointObjectId, out var endpointObject))
                throw new OrchestrationException("The API endpoint configuration object is missing from the request's objects.");

            var endpoint = endpointObject is JsonElement endpointJsonElement
                ? endpointJsonElement.Deserialize<APIEndpointConfiguration>()
                : endpointObject as APIEndpointConfiguration;

            if (endpoint == null
                || string.IsNullOrWhiteSpace(endpoint.Provider)
                || !APIEndpointProviders.All.Contains(endpoint.Provider)
                || string.IsNullOrWhiteSpace(endpoint.Url))
                throw new OrchestrationException("The API endpoint configuration object provided in the request's objects is invalid.");

            if (string.IsNullOrWhiteSpace(Agent.Workflow.MainPromptObjectId))
                throw new OrchestrationException("Invalid prompt object id.");

            if (!Objects.TryGetValue(
                    Agent.Workflow.MainPromptObjectId, out var promptObject))
                throw new OrchestrationException("The prompt object is missing from the request's objects.");

            var prompt = promptObject is JsonElement promptJsonElement
                ? promptJsonElement.Deserialize<MultipartPrompt>()
                : promptObject as MultipartPrompt;

            if (prompt == null
                || string.IsNullOrWhiteSpace(prompt.Prefix))
                throw new OrchestrationException("The prompt object provided in the request's objects is invalid.");

            if (Agent is KnowledgeManagementAgent kmAgent
                && kmAgent.Vectorization != null)
            {
                if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                {
                    if (!Objects.TryGetValue(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId, out var textEmbeddingProfileObject))
                        throw new OrchestrationException("The text embedding profile object is missing from the request's objects.");

                    var textEmbeddingProfile = textEmbeddingProfileObject is JsonElement textEmbeddingProfileJsonElement
                        ? textEmbeddingProfileJsonElement.Deserialize<TextEmbeddingProfile>()
                        : textEmbeddingProfileObject as TextEmbeddingProfile;

                    // All embedding is accomplished via the Gateway with a non-deterministic endpoint, requires the embedding model name (not deployment) in settings.                                       
                    if (textEmbeddingProfile == null)                       
                        throw new OrchestrationException("The text embedding profile object provided in the request's objects is invalid.");

                    if (textEmbeddingProfile.Settings == null)
                        throw new OrchestrationException("The text embedding profile settings must contain a model_name item.");

                    if (textEmbeddingProfile.Settings.ContainsKey(VectorizationSettingsNames.EmbeddingProfileModelName))
                        throw new OrchestrationException("The text embedding profile settings must contain a model_name item.");
                   
                    _hasTextEmbeddingProfile = true;
                }

                if ((kmAgent.Vectorization.IndexingProfileObjectIds ?? []).Count > 0)
                {
                    for (int i = 0; i < kmAgent.Vectorization.IndexingProfileObjectIds!.Count; i++)
                    {
                        var indexingProfileObjectId = kmAgent.Vectorization.IndexingProfileObjectIds[i];

                        if (string.IsNullOrEmpty(indexingProfileObjectId))
                            throw new OrchestrationException($"The indexing profile object id at index {i} is invalid.");

                        if (!Objects.TryGetValue(
                                indexingProfileObjectId, out var indexingProfileObject))
                            throw new OrchestrationException($"The indexing profile object with id {indexingProfileObjectId} is missing from the request's objects.");

                        var indexingProfile = indexingProfileObject is JsonElement indexingProfileJsonElement
                            ? indexingProfileJsonElement.Deserialize<IndexingProfile>()
                            : indexingProfileObject as IndexingProfile;

                        if (indexingProfile == null
                            || indexingProfile.Settings == null
                            || !indexingProfile.Settings.TryGetValue("index_name", out var indexName)
                            || string.IsNullOrWhiteSpace(indexName))
                            throw new OrchestrationException($"The indexing profile object with id {indexingProfileObjectId} provided in the request's objects is invalid.");
                    }

                    _hasIndexingProfiles = true;
                }
            }

            _valid = true;
        }

        /// <summary>
        /// The <see cref="AIModelBase"/> object from the Objects dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// This object is supposed to be added to the Objects dictionary by the instantiator of this request based on the object identifier set on the agent.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public AIModelBase AIModel
        {
            get
            {
                if (_aiModel != null)
                    return _aiModel;

                Validate();

                var aiModelObject = Objects[Agent.Workflow!.MainAIModelObjectId!];
                _aiModel = aiModelObject is JsonElement aiModelJsonElement
                    ? aiModelJsonElement.Deserialize<AIModelBase>()!
                    : (aiModelObject as AIModelBase)!;

                return _aiModel;
            }
        }

        /// <summary>
        /// The <see cref="APIEndpointConfiguration"/> object from the Objects dictionary corresponding to the <see cref="AIModelBase"/> object from the same dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// This object is supposed to be added to the Objects dictionary by the instantiator of this request based on the object identifier set on the AIModel object associated with the agent.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public APIEndpointConfiguration AIModelEndpointConfiguration
        {
            get
            {
                if (_aiModelEndpointConfiguration != null)
                    return _aiModelEndpointConfiguration;

                Validate();

                var endpointObject = Objects[AIModel.EndpointObjectId!];

                _aiModelEndpointConfiguration = endpointObject is JsonElement endpointJsonElement
                    ? endpointJsonElement.Deserialize<APIEndpointConfiguration>()!
                    : (endpointObject as APIEndpointConfiguration)!;

                return _aiModelEndpointConfiguration;
            }
        }

        /// <summary>
        /// The dictionary of other agent descriptions from the Objects dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// This object is supposed to be added to the Objects dictionary by the instantiator of this request based on the descriptions of all agents the calling identity has access to, except for the agent associated with the request.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> OtherAgentsDescriptions
        {
            get
            {
                if (_otherAgentsDescriptions != null)
                    return _otherAgentsDescriptions;

                Validate();

                _otherAgentsDescriptions =
                    Objects.TryGetValue(CompletionRequestObjectsKeys.AllAgents, out var allAgentDescriptions)
                        ? allAgentDescriptions is JsonElement allAgentDescriptionsJsonElement
                            ? allAgentDescriptionsJsonElement.Deserialize<Dictionary<string, string>>()!
                            : (allAgentDescriptions as Dictionary<string, string>)!
                        : [];

                return _otherAgentsDescriptions;
            }
        }

        /// <summary>
        /// The <see cref="MultipartPrompt"/> object from the Objects dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// This object is supposed to be added to the Objects dictionary by the instantiator of this request based on the object identifier set on the agent.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public MultipartPrompt Prompt
        {
            get
            {
                if (_prompt != null)
                    return _prompt;

                Validate();

                var promptObject = Objects[Agent.Workflow!.MainPromptObjectId!];

                _prompt = promptObject is JsonElement promptJsonElement
                ? promptJsonElement.Deserialize<MultipartPrompt>()!
                : (promptObject as MultipartPrompt)!;

                return _prompt;
            }
        }

        /// <summary>
        /// The <see cref="TextEmbeddingProfile"/> object from the Objects dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// This object is supposed to be added to the Objects dictionary by the instantiator of this request based on the object identifier set on the agent's vectorization settings.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public TextEmbeddingProfile? TextEmbeddingProfile
        {
            get
            {
                if (_textEmbeddingProfile != null)
                    return _textEmbeddingProfile;

                Validate();

                if (!_hasTextEmbeddingProfile)
                    return null;

                var textEmbeddingProfileObject = Objects[(Agent as KnowledgeManagementAgent)!.Vectorization.TextEmbeddingProfileObjectId!];

                _textEmbeddingProfile = textEmbeddingProfileObject is JsonElement textEmbeddingProfileJsonElement
                       ? textEmbeddingProfileJsonElement.Deserialize<TextEmbeddingProfile>()!
                       : (textEmbeddingProfileObject as TextEmbeddingProfile)!;

                return _textEmbeddingProfile;
            }
        }

        /// <summary>
        /// The list of <see cref="IndexingProfile"/> objects from the Objects dictionary. Ensure the Validate method is called before accessing this property.
        /// <para>
        /// These objects are supposed to be added to the Objects dictionary by the instantiator of this request based on the object identifiers set on the agent's vectorization settings.
        /// </para>
        /// </summary>
        [JsonIgnore]
        public List<IndexingProfile>? IndexingProfiles
        {
            get
            {
                if (_indexingProfiles != null)
                    return _indexingProfiles;

                Validate();

                if (!_hasIndexingProfiles)
                    return null;

                _indexingProfiles = (Agent as KnowledgeManagementAgent)!.Vectorization.IndexingProfileObjectIds!
                    .Select(indexingProfileObjectId =>
                        {
                            var indexingProfileObject = Objects[indexingProfileObjectId];

                            return indexingProfileObject is JsonElement indexingProfileJsonElement
                                ? indexingProfileJsonElement.Deserialize<IndexingProfile>()!
                                : (indexingProfileObject as IndexingProfile)!;
                        })
                    .ToList();


                return _indexingProfiles;
            }
        }
    }
}
