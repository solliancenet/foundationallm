using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using FoundationaLLM.Vectorization.Models.Configuration;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    /// <summary>
    /// Implements an in-memory request source, suitable for testing and quick prototyping.
    /// </summary>
    /// <param name="settings">The settings used to initialize the request source.</param>
    /// <param name="logger">The logger instnce used for logging.</param>
    public class MemoryRequestSourceService(
        RequestSourceServiceSettings settings,
        ILogger<MemoryRequestSourceService> logger) : IRequestSourceService
    {
        private readonly RequestSourceServiceSettings _settings = settings;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger<MemoryRequestSourceService> _logger = logger;
#pragma warning restore IDE0052 // Remove unread private members
        // contains a list of queued request names
        private readonly ConcurrentQueue<string> _requests = new();

        /// <inheritdoc/>
        public string SourceName => _settings.Name;

        /// <inheritdoc/>
        public Task<bool> HasRequests() =>
            Task.FromResult(!_requests.IsEmpty);

        /// <inheritdoc/>
        public Task<IEnumerable<VectorizationDequeuedRequest>> ReceiveRequests(int count)
        {
            var result = new List<VectorizationDequeuedRequest>();

            for (int i = 0; i < count; i++)
            {
                if (_requests.TryDequeue(out var request))                    
                    result.Add(new VectorizationDequeuedRequest(){
                        RequestName = request,
                        MessageId = string.Empty,
                        PopReceipt = string.Empty,
                        DequeueCount = 0
                    });
                else
                    break;
            }
            
            return Task.FromResult<IEnumerable<VectorizationDequeuedRequest>>(result);
        }

        /// <inheritdoc/>
        public Task DeleteRequest(string requestId, string popReceipt) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task SubmitRequest(string requestName)
        {
            _requests.Enqueue(requestName);
            return Task.CompletedTask;
        }
    }
}
