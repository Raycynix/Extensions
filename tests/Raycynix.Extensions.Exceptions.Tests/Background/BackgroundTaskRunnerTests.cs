using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Tests.Background;

/// <summary>
/// Covers background task execution behavior.
/// </summary>
public sealed class BackgroundTaskRunnerTests
{
    /// <summary>
    /// Verifies that non-transient failures are wrapped into <see cref="InternalServerException"/>.
    /// </summary>
    [Fact]
    public async Task RunAsync_ShouldWrapNonTransientExceptions()
    {
        OperationContext.Current = new OperationContext { CorrelationId = "corr-1", UserId = "user-1" };

        var runner = new BackgroundTaskRunner(
            new PassthroughRetryExecutor(),
            new TransientExceptionClassifier(),
            NullLogger<BackgroundTaskRunner>.Instance);

        var act = async () => await runner.RunAsync(
            _ => throw new InvalidOperationException("boom"),
            "BackgroundSync",
            TestContext.Current.CancellationToken);

        var result = await act.Should().ThrowAsync<InternalServerException>();

        result.Which.Message.Should().Be("Background operation 'BackgroundSync' failed.");
        result.Which.ExecutionContext.Should().NotBeNull();
        result.Which.ExecutionContext!.OperationName.Should().Be("BackgroundSync");
        result.Which.ExecutionContext.CorrelationId.Should().Be("corr-1");
        result.Which.ExecutionContext.UserId.Should().Be("user-1");
    }

    /// <summary>
    /// Verifies that transient failures are propagated as-is after retries are exhausted.
    /// </summary>
    [Fact]
    public async Task RunAsync_ShouldPropagateTransientFailureException()
    {
        var transient = new TransientFailureException("temporary");
        var runner = new BackgroundTaskRunner(
            new ThrowingRetryExecutor(transient),
            new TransientExceptionClassifier(),
            NullLogger<BackgroundTaskRunner>.Instance);

        var act = async () => await runner.RunAsync(
            _ => Task.CompletedTask,
            "BackgroundSync",
            TestContext.Current.CancellationToken);

        var result = await act.Should().ThrowAsync<TransientFailureException>();

        result.Which.Should().BeSameAs(transient);
    }

    /// <summary>
    /// Verifies that request cancellation is propagated without wrapping.
    /// </summary>
    [Fact]
    public async Task RunAsync_ShouldPropagateOperationCanceledException_WhenCancellationIsRequested()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var runner = new BackgroundTaskRunner(
            new ThrowingRetryExecutor(new OperationCanceledException(cts.Token)),
            new TransientExceptionClassifier(),
            NullLogger<BackgroundTaskRunner>.Instance);

        var act = async () => await runner.RunAsync(
            _ => Task.CompletedTask,
            "BackgroundSync",
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private sealed class PassthroughRetryExecutor : IRetryExecutor
    {
        public Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            Raycynix.Extensions.Exceptions.Abstractions.Options.RetryExecutionOptions? options = null,
            string? operationName = null,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }

        public Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            Raycynix.Extensions.Exceptions.Abstractions.Options.RetryExecutionOptions? options = null,
            string? operationName = null,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }

    private sealed class ThrowingRetryExecutor(Exception exception) : IRetryExecutor
    {
        public Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            Raycynix.Extensions.Exceptions.Abstractions.Options.RetryExecutionOptions? options = null,
            string? operationName = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException(exception);
        }

        public Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            Raycynix.Extensions.Exceptions.Abstractions.Options.RetryExecutionOptions? options = null,
            string? operationName = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<T>(exception);
        }
    }
}
