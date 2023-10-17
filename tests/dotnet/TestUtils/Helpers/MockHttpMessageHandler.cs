using Newtonsoft.Json;
using System.Net;

namespace FoundationaLLM.TestUtils.Helpers
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly object _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, object content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode);
            response.Content = new StringContent(JsonConvert.SerializeObject(_content));
            return await Task.FromResult(response);
        }
    }
}
