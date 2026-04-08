using FluentAssertions;
using Raycynix.Extensions.Common.Disposables;

namespace Raycynix.Extensions.Common.Tests.Disposables;

/// <summary>
/// Covers the reusable no-op disposable helper.
/// </summary>
public sealed class NoopDisposableTests
{
    /// <summary>
    /// Verifies that disposing the shared instance is always safe.
    /// </summary>
    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        var action = () => NoopDisposable.Instance.Dispose();

        action.Should().NotThrow();
    }
}
