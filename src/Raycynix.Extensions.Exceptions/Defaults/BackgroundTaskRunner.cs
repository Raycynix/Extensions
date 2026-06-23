using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Executes background operations with retry handling and consistent error translation.
/// </summary>
public class BackgroundTaskRunner(
    IRetryExecutor retryExecutor,
    ITransientExceptionClassifier transientExceptionClassifier,
    ILogger<BackgroundTaskRunner>? logger = null) : IBackgroundTaskRunner
{
    private static readonly RetryExecutionOptions _defaultRetryOptions = new()
    {
        MaxRetries = 3,
        Delay = TimeSpan.FromSeconds(1),
        UseExponentialBackoff = true,
        UseJitter = true
    };

    /// <inheritdoc />
    public async Task RunAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        await RunAsync<object?>(
            async ct =>
            {
                await operation(ct);
                return null;
            },
            operationName,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        var previousContext = ErrorExecutionContextAccessor.Current;
        ErrorExecutionContextAccessor.Current = BuildExecutionContext(operationName, isTransient: false);

        try
        {
            return await retryExecutor.ExecuteAsync(
                operation,
                _defaultRetryOptions,
                operationName,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogInformation("Background operation {OperationName} was canceled.", operationName);
            throw;
        }
        catch (Exception ex) when (transientExceptionClassifier.IsTransient(ex))
        {
            logger?.LogError(ex,
                "Background operation {OperationName} failed due to a transient error after retries were exhausted.",
                operationName);

            throw;
        }
        catch (Exception ex)
        {
            var executionContext = ErrorExecutionContextAccessor.Current ?? BuildExecutionContext(operationName, isTransient: false);

            logger?.LogError(ex,
                "Background operation {OperationName} failed with a non-transient error.",
                operationName);

            throw new InternalServerException(
                message: $"Background operation '{operationName}' failed.",
                innerException: ex,
                secureDetails: new
                {
                    OperationName = operationName,
                    ExceptionType = ex.GetType().FullName,
                    ex.Message
                },
                executionContext: executionContext);
        }
        finally
        {
            ErrorExecutionContextAccessor.Current = previousContext;
        }
    }

    private static ErrorExecutionContext BuildExecutionContext(string operationName, bool isTransient)
    {
        var operationContext = OperationContext.Current;
        return new ErrorExecutionContext(
            Source: "background",
            OperationName: operationName,
            TraceId: Activity.Current?.TraceId.ToString(),
            SpanId: Activity.Current?.SpanId.ToString(),
            CorrelationId: operationContext?.CorrelationId,
            UserId: operationContext?.UserId,
            IsTransient: isTransient);
    }
}
