using FluentAssertions;
using Raycynix.Extensions.Common.Helpers;

namespace Raycynix.Extensions.Common.Tests.Helpers;

/// <summary>
/// Covers assembly metadata helper behavior.
/// </summary>
public sealed class AssemblyHelperTests
{
    /// <summary>
    /// Verifies that the helper resolves a non-empty assembly name.
    /// </summary>
    [Fact]
    public void CurrentName_ShouldReturnNonEmptyValue()
    {
        AssemblyHelper.CurrentName().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that the helper resolves a non-empty assembly version string.
    /// </summary>
    [Fact]
    public void CurrentVersion_ShouldReturnNonEmptyValue()
    {
        AssemblyHelper.CurrentVersion().Should().NotBeNullOrWhiteSpace();
    }
}
