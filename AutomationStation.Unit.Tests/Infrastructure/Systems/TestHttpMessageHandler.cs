using System.Net;

namespace AutomationStation.Unit.Tests.Infrastructure.Systems
{
    public sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public HttpContent? RequestContent { get; private set; }

        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public HttpResponseMessage Response { get; set; } =
            new(HttpStatusCode.OK);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Method = request.Method;
            RequestUri = request.RequestUri;

            RequestContent = request.Content is null
                ? null
                : new StringContent(
                    await request.Content.ReadAsStringAsync(
                        cancellationToken));

            return Response;
        }
    }
}