namespace Raycynix.Extensions.Messaging.HttpJson.Internal;

internal sealed class HttpJsonTransport(IHttpClientFactory httpClientFactory) : IHttpJsonTransport
{
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient(HttpJsonMessagingBuilderExtensions.HttpClientName);
        return client.SendAsync(request, cancellationToken);
    }
}
