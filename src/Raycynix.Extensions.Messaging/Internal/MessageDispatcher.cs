using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageDispatcher(
    IServiceScopeFactory serviceScopeFactory,
    MessagingConfiguration configuration,
    MessageObservability observability) : IMessageDispatcher
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

        var attemptCount = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attemptCount++;

            try
            {
                var handlerCount = await DispatchSingleAttemptAsync(envelope, cancellationToken).ConfigureAwait(false);
                observability.RecordDispatchSuccess(context.MessageType, context.Destination);

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
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                observability.RecordDispatchFailure(context.MessageType, context.Destination);
                throw;
            }
        }
    }

    private async Task<int> DispatchSingleAttemptAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IMessageHandler<TMessage>>().ToArray();

        foreach (var handler in handlers)
        {
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
