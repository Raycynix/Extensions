namespace Raycynix.Extensions.Messaging.HttpJson.Internal;

internal interface IHttpJsonTransport
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
