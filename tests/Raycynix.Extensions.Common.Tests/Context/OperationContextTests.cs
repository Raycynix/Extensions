using FluentAssertions;
using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Common.Tests.Context;

/// <summary>
/// Covers the ambient operation context primitives.
/// </summary>
public sealed class OperationContextTests
{
    /// <summary>
    /// Verifies that correlation identifiers are generated when missing.
    /// </summary>
    [Fact]
    public void CorrelationId_ShouldGenerateValue_WhenMissing()
    {
        var context = new OperationContext();

        context.CorrelationId.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that empty user and subject values are normalized to null.
    /// </summary>
    [Fact]
    public void UserAndSubjectProperties_ShouldNormalizeWhitespaceToNull()
    {
        var context = new OperationContext
        {
            UserId = " ",
            SubjectId = "",
            SubjectType = "   "
        };

        context.UserId.Should().BeNull();
        context.SubjectId.Should().BeNull();
        context.SubjectType.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the correlation identifier is assigned only once through the helper API.
    /// </summary>
    [Fact]
    public void SetCorrelationIdIfMissing_ShouldNotOverwriteExistingValue()
    {
        var context = new OperationContext();
        context.SetCorrelationIdIfMissing("first");
        context.SetCorrelationIdIfMissing("second");

        context.CorrelationId.Should().Be("first");
    }
}
