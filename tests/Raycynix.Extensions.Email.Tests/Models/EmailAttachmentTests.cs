using FluentAssertions;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Tests.Models;

/// <summary>
/// Covers attachment content helper behavior.
/// </summary>
public sealed class EmailAttachmentTests
{
    /// <summary>
    /// Verifies that byte-backed attachments copy content and return fresh streams.
    /// </summary>
    [Fact]
    public async Task FromBytes_ShouldCopyContent_AndOpenFreshStreams()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var bytes = new byte[] { 1, 2, 3 };
        var attachment = EmailAttachment.FromBytes("data.bin", bytes);
        bytes[0] = 9;

        await using var first = await attachment.OpenReadAsync(cancellationToken);
        await using var second = await attachment.OpenReadAsync(cancellationToken);
        var firstBytes = await ReadAllAsync(first);
        var secondBytes = await ReadAllAsync(second);

        first.Should().NotBeSameAs(second);
        firstBytes.Should().BeEquivalentTo([1, 2, 3]);
        secondBytes.Should().BeEquivalentTo([1, 2, 3]);
    }

    /// <summary>
    /// Verifies that file-backed attachments use the file name and open the file lazily.
    /// </summary>
    [Fact]
    public async Task FromFile_ShouldUseFileName_AndReadCurrentFileContent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "hello", cancellationToken);

        try
        {
            var attachment = EmailAttachment.FromFile(path, contentType: "text/plain");

            attachment.FileName.Should().Be(Path.GetFileName(path));
            attachment.ContentType.Should().Be("text/plain");

            await using var stream = await attachment.OpenReadAsync(cancellationToken);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(cancellationToken);

            content.Should().Be("hello");
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Verifies that invalid MIME content types are rejected before provider-specific message creation.
    /// </summary>
    [Fact]
    public void FromBytes_ShouldThrow_WhenContentTypeIsInvalid()
    {
        var act = () => EmailAttachment.FromBytes(
            "data.bin",
            [1, 2, 3],
            "not a content type");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Attachment content type must be a valid MIME content type.*");
    }

    /// <summary>
    /// Verifies that manually constructed attachments validate their MIME content type.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenContentTypeIsInvalid()
    {
        var attachment = new EmailAttachment
        {
            FileName = "data.bin",
            ContentType = "not a content type",
            OpenReadAsync = _ => ValueTask.FromResult<Stream>(new MemoryStream([1, 2, 3]))
        };

        var act = attachment.Validate;

        act.Should().Throw<ArgumentException>()
            .WithMessage("Attachment content type must be a valid MIME content type.*");
    }

    /// <summary>
    /// Verifies that attachment factories honor cancellation before opening content.
    /// </summary>
    [Fact]
    public async Task FromBytes_ShouldThrow_WhenReadIsCancelled()
    {
        var attachment = EmailAttachment.FromBytes("data.bin", [1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var act = async () => await attachment.OpenReadAsync(cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies that MIME-facing attachment values cannot inject additional headers.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenFileNameContainsLineBreak()
    {
        var attachment = new EmailAttachment
        {
            FileName = "data.bin\r\nX-Test: value",
            OpenReadAsync = _ => ValueTask.FromResult<Stream>(new MemoryStream([1, 2, 3]))
        };

        var act = attachment.Validate;

        act.Should().Throw<ArgumentException>()
            .WithMessage("FileName cannot contain line breaks.*");
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, TestContext.Current.CancellationToken);
        return memory.ToArray();
    }
}
