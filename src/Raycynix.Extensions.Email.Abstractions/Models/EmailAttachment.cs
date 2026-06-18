using System.Net.Mime;

namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents an email attachment.
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// Gets or initializes the attachment file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets or initializes the attachment media type.
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// Gets or initializes the attachment content.
    /// </summary>
    public required Func<CancellationToken, ValueTask<Stream>> OpenReadAsync { get; init; }

    /// <summary>
    /// Gets or initializes the optional content identifier used for inline attachments.
    /// </summary>
    public string? ContentId { get; init; }

    /// <summary>
    /// Creates an attachment backed by in-memory content.
    /// </summary>
    /// <param name="fileName">The attachment file name.</param>
    /// <param name="content">The attachment content.</param>
    /// <param name="contentType">The attachment media type.</param>
    /// <param name="contentId">The optional content identifier used for inline attachments.</param>
    /// <returns>The email attachment.</returns>
    public static EmailAttachment FromBytes(
        string fileName,
        byte[] content,
        string contentType = "application/octet-stream",
        string? contentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ValidateContentType(contentType);

        var contentCopy = content.ToArray();

        return new EmailAttachment
        {
            FileName = fileName,
            ContentType = contentType,
            ContentId = contentId,
            OpenReadAsync = _ =>
                ValueTask.FromResult<Stream>(new MemoryStream(contentCopy, writable: false))
        };
    }

    /// <summary>
    /// Creates an attachment backed by in-memory content.
    /// </summary>
    /// <param name="fileName">The attachment file name.</param>
    /// <param name="content">The attachment content.</param>
    /// <param name="contentType">The attachment media type.</param>
    /// <param name="contentId">The optional content identifier used for inline attachments.</param>
    /// <returns>The email attachment.</returns>
    public static EmailAttachment FromMemory(
        string fileName,
        ReadOnlyMemory<byte> content,
        string contentType = "application/octet-stream",
        string? contentId = null)
    {
        return FromBytes(fileName, content.ToArray(), contentType, contentId);
    }

    /// <summary>
    /// Creates an attachment backed by a file that is opened when the attachment is read.
    /// </summary>
    /// <param name="path">The file path to open.</param>
    /// <param name="fileName">The attachment file name. Uses the file name from <paramref name="path"/> when omitted.</param>
    /// <param name="contentType">The attachment media type.</param>
    /// <param name="contentId">The optional content identifier used for inline attachments.</param>
    /// <returns>The email attachment.</returns>
    public static EmailAttachment FromFile(
        string path,
        string? fileName = null,
        string contentType = "application/octet-stream",
        string? contentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ValidateContentType(contentType);

        var resolvedFileName = fileName ?? Path.GetFileName(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(resolvedFileName);

        return new EmailAttachment
        {
            FileName = resolvedFileName,
            ContentType = contentType,
            ContentId = contentId,
            OpenReadAsync = _ => ValueTask.FromResult<Stream>(File.OpenRead(path))
        };
    }

    /// <summary>
    /// Validates the attachment before it is sent.
    /// </summary>
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ContentType);
        ArgumentNullException.ThrowIfNull(OpenReadAsync);
        ValidateContentType(ContentType);
    }

    private static void ValidateContentType(string contentType)
    {
        try
        {
            _ = new ContentType(contentType);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Attachment content type must be a valid MIME content type.", nameof(contentType), exception);
        }
    }
}
