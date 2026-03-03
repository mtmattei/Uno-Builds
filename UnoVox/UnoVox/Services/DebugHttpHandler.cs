using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnoVox.Services
{
    /// <summary>
    /// Simple debug delegating handler that logs outgoing HTTP requests/responses when built in DEBUG.
    /// </summary>
    public sealed class DebugHttpHandler : DelegatingHandler
    {
        public DebugHttpHandler(HttpMessageHandler? innerHandler = null)
        {
            InnerHandler = innerHandler ?? new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
#if DEBUG
            Debug.WriteLine($"[HTTP] -> {request.Method} {request.RequestUri}");
#endif
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
#if DEBUG
            Debug.WriteLine($"[HTTP] <- {(int)response.StatusCode} {response.ReasonPhrase} for {request.RequestUri}");
#endif
            return response;
        }
    }
}
