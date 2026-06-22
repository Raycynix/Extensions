using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    MessagingConfiguration configuration,
    MessageObservability observability,
    IncomingSecurityContextAccessor securityContextAccessor,
    IncomingSecurityContextFactory securityContextFactory,
    ILogger<MessageDispatcher>? logger = null) : IMessageDispatcher
{
    /// <inheritdoc />
    public async ValueTask<MessageDispatchResult> DispatchAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var context = new MessageDispatchContext
        {
            MessageType = typeof(TMessage),
            Destination = envelope.Destination,
            MessageId = envelope.MessageId,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            Contract = envelope.Contract,
            CreatedAt = envelope.CreatedAt
        };

        using var observation = observability.BeginDispatch(context.MessageType, context.Destination);
        logger?.LogDebug(
            "Dispatching message. MessageType={MessageType}, Destination={Destination}, HeaderCount={HeaderCount}.",
            context.MessageType.FullName,
            context.Destination,
            envelope.Headers.Count);

        var attemptCount = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attemptCount++;

            try
            {
                var handlerCount = await DispatchSingleAttemptAsync(envelope, cancellationToken).ConfigureAwait(false);
                observability.RecordDispatchSuccess(context.MessageType, context.Destination);
                logger?.LogDebug(
                    "Message dispatched. MessageType={MessageType}, Destination={Destination}, HandlerCount={HandlerCount}, AttemptCount={AttemptCount}.",
                    context.MessageType.FullName,
                    context.Destination,
                    handlerCount,
                    attemptCount);

                return new MessageDispatchResult
                {
                    Context = context,
                    HandlerCount = handlerCount,
                    AttemptCount = attemptCount
                };
            }
            catch (Exception exception) when (ShouldRetry(exception, attemptCount, cancellationToken))
            {
                var delay = GetRetryDelay(attemptCount);
                logger?.LogWarning(
                    exception,
                    "Message dispatch failed transiently. MessageType={MessageType}, Destination={Destination}, Attempt={Attempt}, RetryDelayMilliseconds={RetryDelayMilliseconds}.",
                    context.MessageType.FullName,
                    context.Destination,
                    attemptCount,
                    delay.TotalMilliseconds);

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                observability.RecordDispatchFailure(context.MessageType, context.Destination);
                logger?.LogError(
                    exception,
                    "Message dispatch failed. MessageType={MessageType}, Destination={Destination}, AttemptCount={AttemptCount}.",
                    context.MessageType.FullName,
                    context.Destination,
                    attemptCount);
                throw;
            }
        }
    }

    private async Task<int> DispatchSingleAttemptAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken)
    {
        using var securityContextScope = securityContextAccessor.Push(securityContextFactory.Create(envelope.Headers));
        using var scope = serviceScopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IMessageHandler<TMessage>>().ToArray();
        var authorizationEvaluator = scope.ServiceProvider.GetRequiredService<MessagingAuthorizationEvaluator>();

        foreach (var handler in handlers)
        {
            authorizationEvaluator.Authorize(handler.GetType(), envelope.Headers);
            await handler.HandleAsync(envelope, cancellationToken).ConfigureAwait(false);
        }

        return handlers.Length;
    }

    private bool ShouldRetry(Exception exception, int attemptCount, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            return false;
        }

        if (exception is IncomingMessageAuthenticationException or IncomingMessageAuthorizationException)
        {
            return false;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (!configuration.DispatchRetry.Enabled)
        {
            return false;
        }

        return attemptCount <= configuration.DispatchRetry.MaxRetries;
    }

    private TimeSpan GetRetryDelay(int attemptCount)
    {
        var delay = configuration.DispatchRetry.Delay;
        if (delay <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        if (!configuration.DispatchRetry.UseExponentialBackoff)
        {
            return delay;
        }

        var multiplier = Math.Pow(2, attemptCount - 1);
        var nextDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * multiplier);
        return nextDelay;
    }
}