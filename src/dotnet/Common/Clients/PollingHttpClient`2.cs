using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides a generic HTTP client that can be used to poll for a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the payload to send to start the operation.</typeparam>
    /// <typeparam name="TResponse">The type of the response received when the operation is completed.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the requests.</param>
    /// <param name="request">The <typeparamref name="TRequest"/> request to send to the service.</param>
    /// <param name="operationStarterPath">The path to send the request to (will be appended to the base path of the HTTP client.</param>
    /// <param name="pollingInterval">The <see cref="TimeSpan"/> interval to poll for the response.</param>
    /// <param name="maxWaitTime">The <see cref="TimeSpan"/> maximum time to wait for the response.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class PollingHttpClient<TRequest, TResponse>(
        HttpClient httpClient,
        TRequest? request,
        string operationStarterPath,
        TimeSpan pollingInterval,
        TimeSpan maxWaitTime,
        ILogger logger)
        where TRequest : CompletionRequestBase
        where TResponse : CompletionResponseBase
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly TRequest? _request = request;
        private readonly string _operationStarterPath = operationStarterPath;
        private readonly TimeSpan _pollingInterval = pollingInterval;
        private readonly TimeSpan _maxWaitTime = maxWaitTime;
        private readonly ILogger _logger = logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions(
            options => {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                return options;
            });

        /// <summary>
        /// Starts an operation and returns immediately.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> indicating the need to cancel the process.</param>
        /// <returns>A <see cref="LongRunningOperation"/>object providing details about the newly started operation.</returns>
        public async Task<LongRunningOperation> StartOperationAsync(CancellationToken cancellationToken = default)
        {
            var body = JsonSerializer.Serialize(_request, _jsonSerializerOptions);
            var responseMessage = await _httpClient.PostAsync(
                _operationStarterPath,
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"),
                cancellationToken);

            if (responseMessage.StatusCode != HttpStatusCode.Accepted)
            {
                _logger.LogError("The operation could not be started. The response status code was {StatusCode}.", responseMessage.StatusCode);
                return new LongRunningOperation
                {
                    OperationId = _request!.OperationId,
                    Status = OperationStatus.Failed,
                    StatusMessage = $"The operation could not be started. The response status code was {responseMessage.StatusCode}."
                };
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var runningOperation = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

            return runningOperation;
        }

        /// <summary>
        /// Gets the status of a running operation.
        /// </summary>
        /// <param name="operationId">The identifier of the running operation.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> indicating the need to cancel the process.</param>
        /// <returns>A <see cref="LongRunningOperation"/>object providing details about the running operation.</returns>
        public async Task<LongRunningOperation> GetOperationStatusAsync(string operationId, CancellationToken cancellationToken = default)
        {
            var operationStatusPath = $"{_operationStarterPath}/{operationId}/status";
            var responseMessage = await _httpClient.GetAsync(operationStatusPath, cancellationToken);

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("The operation was not found. The operation id was {OperationId}.", operationId);
                return new LongRunningOperation
                {
                    OperationId = operationId,
                    Status = OperationStatus.Failed,
                    StatusMessage = $"The operation was not found. The operation id was {operationId}."
                };
            }
            if (responseMessage.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("An error occurred while polling for the response. The response status code was {StatusCode}.", responseMessage.StatusCode);
                return new LongRunningOperation
                {
                    OperationId = operationId,
                    Status = OperationStatus.Failed,
                    StatusMessage = $"An error occurred while polling for the response. The response status code was {responseMessage.StatusCode}."
                };
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var runningOperation = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

            return runningOperation;
        }

        /// <summary>
        /// <para>Executes an operation and waits for the response using a polling mechanism.
        /// The polling mechanism is based on the following assumptions:</para>
        /// - The {operationStarterPath} endpoint will accept a POST with a <typeparamref name="TRequest"/> object as payload and will return a 202 Accepted status code when the operation is started.<br/>
        /// - The returned response will contain a LongRunningOperation object with the operation id.<br/>
        /// - The polling endpoint is available at {operationStarterPath}/{operationId}/status.<br/>
        /// - The polling endpoint will return a 200 status code when the operation is found.<br/>
        /// - The returned response will contain a LongRunningOperation object with the current status of the operation, possibly including the result if the operation completed.<br/>
        /// - The polling endpoint will return a 404 Not Found status code when the operation is not found.<br/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> indicating the need to cancel the process.</param>
        /// <returns>The <typeparamref name="TResponse"/> object containing the response.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TResponse?> ExecuteOperationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var runningOperation = await StartOperationAsync(cancellationToken);

                if (runningOperation.Status == OperationStatus.Failed)
                    return default;

                _logger.LogInformation("The operation was started successfully. The operation id is {OperationId}.", runningOperation.OperationId);

                var pollingStartTime = DateTime.UtcNow;
                var pollingCounter = 0;

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("The operation was cancelled. The operation id was {OperationId}.", runningOperation.OperationId);
                        return default;
                    }

                    var totalPollingTime = DateTime.UtcNow - pollingStartTime;
                    pollingCounter++;
                    _logger.LogInformation(
                        "Polling for operation id {Operationid} (counter: {PollingCounter}, time elapsed: {PollingSeconds} seconds)...",
                        runningOperation.OperationId,
                        pollingCounter,
                        (int)totalPollingTime.TotalSeconds);

                    var operationStatus = await GetOperationStatusAsync(runningOperation.OperationId!, cancellationToken);

                    switch (operationStatus.Status)
                    {
                        case OperationStatus.Completed:
                            if (operationStatus.Result is JsonElement jsonElement)
                            {
                                return jsonElement.Deserialize<TResponse>();
                            }
                            return default;
                        case OperationStatus.InProgress:
                            if (totalPollingTime > _maxWaitTime)
                            {
                                _logger.LogWarning("Total polling time ({TotalTime} seconds) exceeded to maximum allowed ({MaxTime} seconds).",
                                    totalPollingTime.TotalSeconds,
                                    _maxWaitTime.TotalSeconds);                                
                                return default;
                            }
                            
                            break;
                        default:
                            _logger.LogError("The operation status {OperationStatus} is not supported.", operationStatus.Status);
                            return default;
                    }
                    await Task.Delay(_pollingInterval, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while polling for the response.");
                return default;
            }
        }
    }
}
