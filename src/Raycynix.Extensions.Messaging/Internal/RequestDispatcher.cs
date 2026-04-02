using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.Internal;

/// <summary>
/// Dispatches direct request envelopes to registered request handlers.
/// </summary>
internal sealed class RequestDispatcher(
    IServiceProvider serviceProvider,
    IEnumerable<RequestHandlerRegistration> registrations,
    Configurations.MessagingConfiguration configuration,
    IncomingSecurityHeadersValidator securityHeadersValidator,
    IncomingSecurityContextAccessor securityContextAccessor,
    IncomingSecurityContextFactory securityContextFactory,
    MessageObservability observability) : IRequestDispatcher
{
    /// <inheritdoc />
    public async ValueTask<ResponseEnvelope<TResponse>> DispatchAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (configuration.IncomingProcessing.ValidateSecurityHeaders)
        {
            securityHeadersValidator.Validate(request.Headers);
        }

        using var observation = observability.BeginRequest(typeof(TRequest), request.Destination);

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

        var matchedRegistration = matchingRegistrations[0];
        using var securityContextScope = securityContextAccessor.Push(securityContextFactory.Create(request.Headers));
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService(matchedRegistration.HandlerType) as IRequestHandler<TRequest, TResponse>;
        if (handler is null)
        {
            throw new InvalidOperationException(
                $"The request handler '{matchedRegistration.HandlerType.FullName}' registered for destination '{request.Destination}' does not implement '{typeof(IRequestHandler<TRequest, TResponse>).FullName}'.");
        }

        try
        {
            scope.ServiceProvider.GetRequiredService<MessagingAuthorizationEvaluator>().Authorize(matchedRegistration.HandlerType, request.Headers);
            var response = await handler.HandleAsync(request, cancellationToken).ConfigureAwait(false);
            observability.RecordRequestSuccess(typeof(TRequest), request.Destination);
            return response;
        }
        catch
        {
            observability.RecordRequestFailure(typeof(TRequest), request.Destination);
            throw;
        }
    }
}
