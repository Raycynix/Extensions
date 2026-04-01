namespace Raycynix.Extensions.Messaging.HttpJson.Interfaces;

/// <summary>
/// Sends raw HTTP requests for the HTTP JSON direct transport.
/// </summary>
public interface IHttpJsonTransport
{
    /// <summary>
    /// Sends the supplied HTTP request message.
    /// </summary>
    /// <param name="request">The request message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The received HTTP response message.</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}
