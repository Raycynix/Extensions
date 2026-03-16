using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Executes asynchronous operations with retry support for transient failures.
/// </summary>
public class RetryExecutor(
    ITransientExceptionClassifier transientExceptionClassifier,
    ILogger<RetryExecutor> logger) : IRetryExecutor
{
    private static readonly Random _sharedRandom = new();

    /// <inheritdoc />
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RetryExecutionOptions();
        options.Validate();

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

    /// <inheritdoc />
    public Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RetryExecutionOptions();
        options.Validate();

        return ExecuteCoreAsync(operation, options, operationName, cancellationToken);
    }

    private async Task<T> ExecuteCoreAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryExecutionOptions options,
        string? operationName,
        CancellationToken cancellationToken)
    {
        var actualOperationName = string.IsNullOrWhiteSpace(operationName) ? "UnnamedOperation" : operationName;
        var attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested &&
                                       transientExceptionClassifier.IsTransient(ex) &&
                                       attempt <= options.MaxRetries)
            {
                var delay = CalculateDelay(options, attempt);

                logger.LogWarning(ex,
                    "Transient failure during {OperationName}. Attempt {Attempt}/{MaxAttempts}. Retrying in {DelayMs} ms.",
                    actualOperationName,
                    attempt,
                    options.MaxRetries + 1,
                    delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested &&
                                       transientExceptionClassifier.IsTransient(ex))
            {
                logger.LogError(ex,
                    "Transient failure during {OperationName}. Retry limit reached after {Attempts} attempts.",
                    actualOperationName,
                    attempt);

                throw new TransientFailureException(
                    message: $"Operation '{actualOperationName}' failed after retry attempts were exhausted.",
                    secureDetails: new
                    {
                        OperationName = actualOperationName,
                        Attempts = attempt,
                        OriginalException = ex.Message
                    },
                    innerException: ex);
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

        var jitter = _sharedRandom.Next(0, 250);
        return delay + TimeSpan.FromMilliseconds(jitter);
    }
}