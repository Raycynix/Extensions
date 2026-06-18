using FluentAssertions;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Tests.Models;

/// <summary>
/// Covers send result factory invariants.
/// </summary>
public sealed class EmailSendResultTests
{
    /// <summary>
    /// Verifies that successful results cannot carry failure state.
    /// </summary>
    [Fact]
    public void Success_ShouldCreateAcceptedResult()
    {
        var result = EmailSendResult.Success("smtp", "message-1");

        result.Succeeded.Should().BeTrue();
        result.Provider.Should().Be("smtp");
        result.MessageId.Should().Be("message-1");
        result.ErrorCode.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    /// <summary>
    /// Verifies that failure results require a provider and an error message.
    /// </summary>
    [Fact]
    public void Failure_ShouldCreateRejectedResult()
    {
        var result = EmailSendResult.Failure("smtp", "Rejected", "MailboxUnavailable");

        result.Succeeded.Should().BeFalse();
        result.Provider.Should().Be("smtp");
        result.MessageId.Should().BeNull();
        result.ErrorCode.Should().Be("MailboxUnavailable");
        result.ErrorMessage.Should().Be("Rejected");
    }

    /// <summary>
    /// Verifies that metadata is copied so external dictionary changes do not mutate the result.
    /// </summary>
    [Fact]
    public void Success_ShouldCopyMetadata()
    {
        var metadata = new Dictionary<string, string> { ["trace"] = "first" };

        var result = EmailSendResult.Success("smtp", metadata: metadata);
        metadata["trace"] = "second";

        result.Metadata["trace"].Should().Be("first");
    }
}
