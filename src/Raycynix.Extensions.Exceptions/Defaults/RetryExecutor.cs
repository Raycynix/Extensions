using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Executes asynchronous operations with retry support for transient failures.
/// </summary>
public class RetryExecutor(
    ITransientExceptionClassifier transientExceptionClassifier,
    ILogger<RetryExecutor>? logger = null) : IRetryExecutor
{
    /// <inheritdoc />
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        options ??= new RetryExecutionOptions();
        options.Validate();

        var previousContext = ErrorExecutionContextAccessor.Current;

        try
        {
            await ExecuteCoreAsync<object?>(
                async ct =>
                {
                    await operation(ct);
                    return null;
                },
                options,
                operationName,
                cancellationToken);
        }
        finally
        {
            ErrorExecutionContextAccessor.Current = previousContext;
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        options ??= new RetryExecutionOptions();
        options.Validate();

        var previousContext = ErrorExecutionContextAccessor.Current;

        try
        {
            return await ExecuteCoreAsync(operation, options, operationName, cancellationToken);
        }
        finally
        {
            ErrorExecutionContextAccessor.Current = previousContext;
        }
    }

    private async Task<T> ExecuteCoreAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryExecutionOptions options,
        string? operationName,
        CancellationToken cancellationToken)
    {
        var actualOperationName = string.IsNullOrWhiteSpace(operationName) ? "UnnamedOperation" : operationName;
        var attempt = 0;
        var maxAttempts = options.MaxRetries + 1;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;
            SetCurrentExecutionContext(actualOperationName, attempt, maxAttempts, isTransient: false);

            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested &&
                                       transientExceptionClassifier.IsTransient(ex) &&
                                       attempt <= options.MaxRetries)
            {
                var delay = CalculateDelay(options, attempt);
                SetCurrentExecutionContext(actualOperationName, attempt, maxAttempts, isTransient: true);

                logger?.LogWarning(ex,
                    "Transient failure during {OperationName}. Attempt {Attempt}/{MaxAttempts}. Retrying in {DelayMs} ms.",
                    actualOperationName,
                    attempt,
                    maxAttempts,
                    delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested &&
                                       transientExceptionClassifier.IsTransient(ex))
            {
                var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(options.Delay.TotalSeconds));
                var executionContext = BuildExecutionContext(actualOperationName, attempt, maxAttempts, isTransient: true);
                SetCurrentExecutionContext(actualOperationName, attempt, maxAttempts, isTransient: true);

                logger?.LogError(ex,
                    "Transient failure during {OperationName}. Retry limit reached after {Attempts} attempts.",
                    actualOperationName,
                    attempt);

                throw new TransientFailureException(
                    message: $"Operation '{actualOperationName}' failed after retry attempts were exhausted.",
                    secureDetails: new
                    {
                        OperationName = actualOperationName,
                        Attempts = attempt,
                        MaxAttempts = maxAttempts,
                        RetryAfterSeconds = retryAfterSeconds,
                        OriginalException = ex.Message
                    },
                    innerException: ex,
                    operationName: actualOperationName,
                    attemptCount: attempt,
                    maxAttempts: maxAttempts,
                    retryAfterSeconds: retryAfterSeconds,
                    executionContext: executionContext);
            }
        }
    }

    private static TimeSpan CalculateDelay(RetryExecutionOptions options, int attempt)
    {
        var multiplier = options.UseExponentialBackoff
            ? Math.Pow(2, Math.Max(0, attempt - 1))
            : 1;

        var delay = TimeSpan.FromMilliseconds(options.Delay.TotalMilliseconds * multiplier);

        if (!options.UseJitter)
        {
            return delay;
        }

        var jitter = Random.Shared.Next(0, 250);
        return delay + TimeSpan.FromMilliseconds(jitter);
    }

    private static void SetCurrentExecutionContext(
        string operationName,
        int attempt,
        int maxAttempts,
        bool isTransient)
    {
        ErrorExecutionContextAccessor.Current = BuildExecutionContext(operationName, attempt, maxAttempts, isTransient);
    }

    private static ErrorExecutionContext BuildExecutionContext(
        string operationName,
        int attempt,
        int maxAttempts,
        bool isTransient)
    {
        var operationContext = OperationContext.Current;
        return new ErrorExecutionContext(
            Source: "background",
            OperationName: operationName,
            TraceId: Activity.Current?.TraceId.ToString(),
            SpanId: Activity.Current?.SpanId.ToString(),
            CorrelationId: operationContext?.CorrelationId,
            UserId: operationContext?.UserId,
            Attempt: attempt,
            MaxAttempts: maxAttempts,
            IsTransient: isTransient);
    }
}
