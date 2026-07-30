using FluentAssertions;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

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

    [Fact]
    public void RaycynixException_ShouldCopyDetails()
    {
        var details = new List<IExceptionDetail>
        {
            new ExceptionDetail("first", "First detail")
        };
        var exception = new TestException(details);

        details.Add(new ExceptionDetail("second", "Second detail"));

        exception.Details.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RaycynixException_ShouldRejectBlankErrorCode(string errorCode)
    {
        var act = () => new TestException(errorCode: errorCode);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidationException_ShouldCopyValidationErrors()
    {
        var messages = new[] { "Required" };
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = messages
        };
        var exception = new ValidationException("Invalid request.", errors);

        messages[0] = "Changed";
        errors["Other"] = ["Unexpected"];

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors["Name"].Should().ContainSingle().Which.Should().Be("Required");
    }

    [Theory]
    [InlineData("", "Message")]
    [InlineData("code", "")]
    public void ExceptionDetail_ShouldRejectBlankRequiredValues(string code, string message)
    {
        var act = () => new ExceptionDetail(code, message);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidationException_ShouldRejectBlankValidationMessage()
    {
        var act = () => new ValidationException(
            "Invalid request.",
            new Dictionary<string, string[]>
            {
                ["Name"] = [" "]
            });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TransientFailureException_ShouldRejectNegativeRetryDelay()
    {
        var act = () => new TransientFailureException(retryAfterSeconds: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private sealed class TestException(
        IReadOnlyCollection<IExceptionDetail>? details = null,
        string errorCode = "TEST_ERROR")
        : RaycynixException(
            "Test exception.",
            errorCode,
            500,
            ErrorCategory.Internal,
            details);
}
