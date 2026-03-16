using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Executes operations with retry semantics for transient failures.
/// </summary>
public interface IRetryExecutor
{
    /// <summary>
    /// Executes an asynchronous operation with retry support.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="options">The retry options.</param>
    /// <param name="operationName">The logical operation name used for logging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous operation with retry support and returns its result.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="options">The retry options.</param>
    /// <param name="operationName">The logical operation name used for logging.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        RetryExecutionOptions? options = null,
        string? operationName = null,
        CancellationToken cancellationToken = default);
}