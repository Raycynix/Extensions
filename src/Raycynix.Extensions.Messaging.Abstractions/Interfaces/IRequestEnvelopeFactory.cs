using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Creates direct request envelopes with consistent metadata.
/// </summary>
public interface IRequestEnvelopeFactory
{
    /// <summary>
    /// Creates a request envelope.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <param name="request">The request payload.</param>
    /// <param name="destination">The logical route, path, or method name.</param>
    /// <param name="format">The payload format.</param>
    /// <param name="headers">Optional headers.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="timeout">Optional request timeout.</param>
    /// <returns>The request envelope.</returns>
    RequestEnvelope<TRequest> Create<TRequest>(
        TRequest request,
        string destination,
        MessageFormat format,
        IReadOnlyDictionary<string, string>? headers = null,
        string? correlationId = null,
        TimeSpan? timeout = null);
}
