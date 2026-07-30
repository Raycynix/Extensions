using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Options;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Tests.Retry;

/// <summary>
/// Covers retry execution behavior for transient and non-transient failures.
/// </summary>
public sealed class RetryExecutorTests
{
    /// <summary>
    /// Verifies that transient failures are retried and can eventually succeed.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldRetryTransientFailure_AndEventuallySucceed()
    {
        var classifier = new TransientExceptionClassifier();
        var executor = new RetryExecutor(classifier, NullLogger<RetryExecutor>.Instance);
        var attempts = 0;

        await executor.ExecuteAsync(
            _ =>
            {
                attempts++;

                return attempts == 1 ? throw new TimeoutException("retry me") : Task.CompletedTask;
            },
            new RetryExecutionOptions
            {
                MaxRetries = 2,
                Delay = TimeSpan.Zero,
                UseExponentialBackoff = false,
                UseJitter = false
            },
            "RetryingOperation",
            TestContext.Current.CancellationToken);

        attempts.Should().Be(2);
    }

    /// <summary>
    /// Verifies that exhausted transient failures are wrapped into <see cref="TransientFailureException"/>.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowTransientFailureException_WhenRetriesAreExhausted()
    {
        var classifier = new TransientExceptionClassifier();
        var executor = new RetryExecutor(classifier, NullLogger<RetryExecutor>.Instance);
        var attempts = 0;

        var act = async () => await executor.ExecuteAsync(
            _ =>
            {
                attempts++;
                throw new TimeoutException("retry me");
            },
            new RetryExecutionOptions
            {
                MaxRetries = 1,
                Delay = TimeSpan.Zero,
                UseExponentialBackoff = false,
                UseJitter = false
            },
            "RetryingOperation",
            TestContext.Current.CancellationToken);

        var result = await act.Should().ThrowAsync<TransientFailureException>();

        attempts.Should().Be(2);
        result.Which.OperationName.Should().Be("RetryingOperation");
        result.Which.AttemptCount.Should().Be(2);
        result.Which.MaxAttempts.Should().Be(2);
    }

    /// <summary>
    /// Verifies that cancellation is observed before execution starts.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_ShouldThrowOperationCanceledException_WhenCancellationIsRequested()
    {
        var classifier = new TransientExceptionClassifier();
        var executor = new RetryExecutor(classifier, NullLogger<RetryExecutor>.Instance);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await executor.ExecuteAsync(
            _ => Task.CompletedTask,
            new RetryExecutionOptions
            {
                MaxRetries = 0,
                Delay = TimeSpan.Zero,
                UseExponentialBackoff = false,
                UseJitter = false
            },
            "CanceledOperation",
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectNullOperation()
    {
        var executor = new RetryExecutor(
            new TransientExceptionClassifier(),
            NullLogger<RetryExecutor>.Instance);

        var act = () => executor.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
