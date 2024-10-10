using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IGatekeeperService"/> interface.
    /// </summary>
    /// <remarks>
    /// Constructor for the Gatekeeper service.
    /// </remarks>
    /// <param name="orchestrationAPIService">The Orchestration API client.</param>
    /// <param name="cosmosDBService">The Azure Cosmos DB service.</param>
    /// <param name="contentSafetyService">The user prompt Content Safety service.</param>
    /// <param name="lakeraGuardService">The Lakera Guard service.</param>
    /// <param name="enkryptGuardrailsService">The Enkrypt Guardrails service.</param>
    /// <param name="gatekeeperIntegrationAPIService">The Gatekeeper Integration API client.</param>
    /// <param name="gatekeeperServiceSettings">The configuration options for the Gatekeeper service.</param>
    public class GatekeeperService(
        IDownstreamAPIService orchestrationAPIService,
        IAzureCosmosDBService cosmosDBService,
        IContentSafetyService contentSafetyService,
        ILakeraGuardService lakeraGuardService,
        IEnkryptGuardrailsService enkryptGuardrailsService,
        IGatekeeperIntegrationAPIService gatekeeperIntegrationAPIService,
        IOptions<GatekeeperServiceSettings> gatekeeperServiceSettings) : IGatekeeperService
    {
        private readonly IDownstreamAPIService _orchestrationAPIService = orchestrationAPIService;
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
        private readonly IContentSafetyService _contentSafetyService = contentSafetyService;
        private readonly ILakeraGuardService _lakeraGuardService = lakeraGuardService;
        private readonly IEnkryptGuardrailsService _enkryptGuardrailsService = enkryptGuardrailsService;
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = gatekeeperIntegrationAPIService;
        private readonly GatekeeperServiceSettings _gatekeeperServiceSettings = gatekeeperServiceSettings.Value;

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        public async Task<CompletionResponse> GetCompletion(string instanceId, CompletionRequest completionRequest)
        {
            if (completionRequest.GatekeeperOptions != null && completionRequest.GatekeeperOptions.Length > 0)
            {
                _gatekeeperServiceSettings.EnableAzureContentSafety = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafety);
                _gatekeeperServiceSettings.EnableMicrosoftPresidio = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.MicrosoftPresidio);
                _gatekeeperServiceSettings.EnableLakeraGuard = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.LakeraGuard);
                _gatekeeperServiceSettings.EnableEnkryptGuardrails = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.EnkryptGuardrails);
                _gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafetyPromptShield);
            }

            if (_gatekeeperServiceSettings.EnableLakeraGuard)
            {
                var promptInjectionResult = await _lakeraGuardService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { OperationId = completionRequest.OperationId, Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableEnkryptGuardrails)
            {
                var promptInjectionResult = await _enkryptGuardrailsService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { OperationId = completionRequest.OperationId, Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield)
            {
                var promptInjectionResult = await _contentSafetyService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new CompletionResponse() { OperationId = completionRequest.OperationId, Completion = promptInjectionResult };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new CompletionResponse() { OperationId = completionRequest.OperationId, Completion = contentSafetyResult.Reason };
            }

            var completionResponse = await _orchestrationAPIService.GetCompletion(instanceId, completionRequest);

            if (_gatekeeperServiceSettings.EnableMicrosoftPresidio)
                completionResponse.Completion = await _gatekeeperIntegrationAPIService.AnonymizeText(completionResponse.Completion);

            return completionResponse;
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
        {
            if (completionRequest.GatekeeperOptions != null && completionRequest.GatekeeperOptions.Length > 0)
            {
                await _cosmosDBService.UpdateLongRunningOperationContextPropertiesAsync(
                    completionRequest.OperationId!,
                    new Dictionary<string, object>
                    {
                        { "/gatekeeperOptions", completionRequest.GatekeeperOptions }
                    });

                _gatekeeperServiceSettings.EnableAzureContentSafety = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafety);
                _gatekeeperServiceSettings.EnableLakeraGuard = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.LakeraGuard);
                _gatekeeperServiceSettings.EnableEnkryptGuardrails = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.EnkryptGuardrails);
                _gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield = completionRequest.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.AzureContentSafetyPromptShield);
            }

            if (_gatekeeperServiceSettings.EnableLakeraGuard)
            {
                var promptInjectionResult = await _lakeraGuardService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new LongRunningOperation() { OperationId = completionRequest.OperationId, StatusMessage = promptInjectionResult, Status = OperationStatus.Failed };
            }

            if (_gatekeeperServiceSettings.EnableEnkryptGuardrails)
            {
                var promptInjectionResult = await _enkryptGuardrailsService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new LongRunningOperation() { OperationId = completionRequest.OperationId, StatusMessage = promptInjectionResult, Status = OperationStatus.Failed };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafetyPromptShield)
            {
                var promptInjectionResult = await _contentSafetyService.DetectPromptInjection(completionRequest.UserPrompt!);

                if (!string.IsNullOrWhiteSpace(promptInjectionResult))
                    return new LongRunningOperation() { OperationId = completionRequest.OperationId, StatusMessage = promptInjectionResult, Status = OperationStatus.Failed };
            }

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new LongRunningOperation() { OperationId = completionRequest.OperationId, StatusMessage = contentSafetyResult.Reason, Status = OperationStatus.Failed };
            }

            var response = await _orchestrationAPIService.StartCompletionOperation(instanceId, completionRequest);

            return response;
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
        {
            var operationStatus = await _orchestrationAPIService.GetCompletionOperationStatus(instanceId, operationId);

            var operationContext = await _cosmosDBService.GetLongRunningOperationContextAsync(operationId);

            _gatekeeperServiceSettings.EnableMicrosoftPresidio = operationContext.GatekeeperOptions.Any(x => x == GatekeeperOptionNames.MicrosoftPresidio);

            if (operationStatus.Status == OperationStatus.Completed
                && operationStatus.Result != null
                && operationStatus.Result is CompletionResponse completionResponse
                && _gatekeeperServiceSettings.EnableMicrosoftPresidio)
            {
                completionResponse.Completion = await _gatekeeperIntegrationAPIService.AnonymizeText(completionResponse.Completion);
                operationStatus.Result = completionResponse;
            }

            return operationStatus;
        }
    }
}
