using FluentAssertions;
using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Tests.Options;

/// <summary>
/// Covers validation behavior for retry execution options.
/// </summary>
public sealed class RetryExecutionOptionsTests
{
    /// <summary>
    /// Verifies that default options are valid.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_ForDefaultOptions()
    {
        var options = new RetryExecutionOptions();

        var act = options.Validate;

        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that negative retry counts are rejected.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenMaxRetriesIsNegative()
    {
        var options = new RetryExecutionOptions
        {
            MaxRetries = -1
        };

        var act = options.Validate;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that negative delays are rejected.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenDelayIsNegative()
    {
        var options = new RetryExecutionOptions
        {
            Delay = TimeSpan.FromSeconds(-1)
        };

        var act = options.Validate;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMaxDelayIsNotPositive()
    {
        var options = new RetryExecutionOptions { MaxDelay = TimeSpan.Zero };

        var act = options.Validate;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMaxRetriesIsUnreasonablyLarge()
    {
        var options = new RetryExecutionOptions { MaxRetries = 1001 };

        var act = options.Validate;

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
