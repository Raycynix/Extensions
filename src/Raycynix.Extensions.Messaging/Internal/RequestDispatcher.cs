using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Dispatches direct request envelopes to registered request handlers.
/// </summary>
internal sealed class RequestDispatcher(
    IServiceProvider serviceProvider,
    IEnumerable<RequestHandlerRegistration> registrations) : IRequestDispatcher
{
    /// <inheritdoc />
    public async ValueTask<ResponseEnvelope<TResponse>> DispatchAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var matchingRegistrations = registrations
            .Where(candidate =>
                string.Equals(candidate.Destination, request.Destination, StringComparison.OrdinalIgnoreCase) &&
                candidate.RequestType == typeof(TRequest) &&
                candidate.ResponseType == typeof(TResponse))
            .ToArray();

        if (matchingRegistrations.Length == 0)
        {
            throw new InvalidOperationException(
                $"No request handler is registered for destination '{request.Destination}' and types '{typeof(TRequest).FullName}'/'{typeof(TResponse).FullName}'.");
        }

        if (matchingRegistrations.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple request handlers are registered for destination '{request.Destination}' and types '{typeof(TRequest).FullName}'/'{typeof(TResponse).FullName}'.");
        }

        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IRequestHandler<TRequest, TResponse>>().ToArray();
        if (handlers.Length != 1)
        {
            throw new InvalidOperationException(
                $"Exactly one request handler implementation must be registered for destination '{request.Destination}' and types '{typeof(TRequest).FullName}'/'{typeof(TResponse).FullName}'.");
        }

        return await handlers[0].HandleAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
