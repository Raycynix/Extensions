namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Provides a safe wrapper for executing background operations with unified error handling.
/// </summary>
public interface IBackgroundTaskRunner
{
    /// <summary>
    /// Executes a background operation with logging and transient failure handling.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The logical operation name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RunAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a background operation with logging and transient failure handling and returns a result.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The logical operation name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result.</returns>
    Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default);
}