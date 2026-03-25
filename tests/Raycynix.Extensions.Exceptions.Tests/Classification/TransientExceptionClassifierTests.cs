using FluentAssertions;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Tests.Classification;

/// <summary>
/// Covers transient exception classification rules.
/// </summary>
public sealed class TransientExceptionClassifierTests
{
    /// <summary>
    /// Verifies that common transient exception types are recognized.
    /// </summary>
    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(TaskCanceledException))]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(HttpRequestException))]
    public void IsTransient_ShouldReturnTrue_ForKnownTransientTypes(Type exceptionType)
    {
        var classifier = new TransientExceptionClassifier();
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        classifier.IsTransient(exception).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that explicit cancellation is not treated as transient.
    /// </summary>
    [Fact]
    public void IsTransient_ShouldReturnFalse_ForOperationCanceledException()
    {
        var classifier = new TransientExceptionClassifier();

        classifier.IsTransient(new OperationCanceledException()).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that transient inner exceptions make the outer exception transient.
    /// </summary>
    [Fact]
    public void IsTransient_ShouldReturnTrue_WhenInnerExceptionIsTransient()
    {
        var classifier = new TransientExceptionClassifier();
        var exception = new InvalidOperationException("outer", new TimeoutException("inner"));

        classifier.IsTransient(exception).Should().BeTrue();
    }
}
