using FluentAssertions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;

namespace Raycynix.Extensions.Exceptions.Tests.Models;

/// <summary>
/// Covers built-in exception type defaults.
/// </summary>
public sealed class ExceptionTypesTests
{
    /// <summary>
    /// Verifies defaults for <see cref="UnauthorizedException"/>.
    /// </summary>
    [Fact]
    public void UnauthorizedException_ShouldExposeExpectedDefaults()
    {
        var exception = new UnauthorizedException();

        exception.StatusCode.Should().Be(401);
        exception.ErrorCode.Should().Be("UNAUTHORIZED");
        exception.Category.Should().Be(ErrorCategory.Unauthorized);
    }

    /// <summary>
    /// Verifies defaults for <see cref="ForbiddenException"/>.
    /// </summary>
    [Fact]
    public void ForbiddenException_ShouldExposeExpectedDefaults()
    {
        var exception = new ForbiddenException();

        exception.StatusCode.Should().Be(403);
        exception.ErrorCode.Should().Be("FORBIDDEN");
        exception.Category.Should().Be(ErrorCategory.Forbidden);
    }

    /// <summary>
    /// Verifies defaults for <see cref="ConflictException"/>.
    /// </summary>
    [Fact]
    public void ConflictException_ShouldExposeExpectedDefaults()
    {
        var exception = new ConflictException("Conflict");

        exception.StatusCode.Should().Be(409);
        exception.ErrorCode.Should().Be("RESOURCE_CONFLICT");
        exception.Category.Should().Be(ErrorCategory.Conflict);
    }

    /// <summary>
    /// Verifies defaults for <see cref="InternalServerException"/>.
    /// </summary>
    [Fact]
    public void InternalServerException_ShouldExposeExpectedDefaults()
    {
        var exception = new InternalServerException("Failure");

        exception.StatusCode.Should().Be(500);
        exception.ErrorCode.Should().Be("INTERNAL_SERVER_ERROR");
        exception.Category.Should().Be(ErrorCategory.Internal);
    }
}
